using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioPrestamo
{
    private const string MensajeSinPermisoParaEvaluar = "El usuario no tiene permiso para evaluar solicitudes";
    private const string MensajeSinPermisoParaMisSolicitudes =
        "El usuario no tiene permiso para consultar sus solicitudes";
    private const string MensajeSinPermisoParaPrestamosActivos =
        "El usuario no tiene permiso para consultar los prestamos activos";
    private const string MensajeSinPermisoParaConsultarPrestamos =
        "El usuario no tiene permiso para consultar prestamos";
    private const string MensajeRangoInvertido = "La fecha desde no puede ser posterior a la fecha hasta";
    private const string MensajeEstadoNoValido = "El estado del prestamo no es valido";
    private const int DiasPorDefecto = 30;

    private readonly RepositorioPrestamoSql _prestamos;
    private readonly RepositorioUsuarioSql _usuarios;
    private readonly RepositorioCatalogoSql _catalogo;
    private readonly RepositorioMovimientoSql _movimientos;
    private readonly ServicioNotificacion _servicioNotificacion;
    private readonly TimeProvider _reloj;
    private readonly RegistradorDeAuditoria _auditoria;

    public ServicioPrestamo(RepositorioPrestamoSql prestamos, RepositorioUsuarioSql usuarios,
        RepositorioCatalogoSql catalogo, RepositorioMovimientoSql movimientos,
        ServicioNotificacion servicioNotificacion, TimeProvider reloj, RegistradorDeAuditoria auditoria)
    {
        _prestamos = prestamos;
        _usuarios = usuarios;
        _catalogo = catalogo;
        _movimientos = movimientos;
        _servicioNotificacion = servicioNotificacion;
        _reloj = reloj;
        _auditoria = auditoria;
    }

    public PrestamoDto Solicitar(SolicitudPrestamoDto datos, string dniSolicitante)
    {
        ArgumentNullException.ThrowIfNull(datos);

        Usuario solicitante = ObtenerSolicitante(dniSolicitante);
        TipoEquipamiento tipo = ObtenerTipoEquipamiento(datos.IdCatalogo, datos.TipoEquipamiento);
        ValidarLimites(solicitante, tipo, datos.CantidadSolicitada);

        Prestamo prestamo = TraduccionDeErrores.Ejecutar(
            () => RegistrarSolicitud(solicitante, tipo, datos.CantidadSolicitada));

        return CrearPrestamoDto(prestamo);
    }

    public PrestamoDto Aprobar(Guid idPrestamo, string dniEvaluador)
    {
        Usuario evaluador = ObtenerUsuarioHabilitado(
                    dniEvaluador, Permiso.EvaluarSolicitud, MensajeSinPermisoParaEvaluar);
        Prestamo prestamo = ObtenerPrestamo(idPrestamo);

        Usuario solicitante = ObtenerSolicitante(prestamo.Solicitante.Dni);

        TraduccionDeErrores.Ejecutar(() => ResolverAprobacion(prestamo, solicitante, evaluador));

        return CrearPrestamoDto(prestamo);
    }

    public PrestamoDto Rechazar(Guid idPrestamo, string motivo, string dniEvaluador)
    {
        Usuario evaluador = ObtenerUsuarioHabilitado(
                    dniEvaluador, Permiso.EvaluarSolicitud, MensajeSinPermisoParaEvaluar);
        Prestamo prestamo = ObtenerPrestamo(idPrestamo);

        TraduccionDeErrores.Ejecutar(() => RegistrarRechazo(prestamo, evaluador, motivo));

        return CrearPrestamoDto(prestamo);
    }

    public List<PrestamoDto> ListarSolicitudesPendientes(string dniConsultante)
    {
        ObtenerUsuarioHabilitado(dniConsultante, Permiso.EvaluarSolicitud, MensajeSinPermisoParaEvaluar);

        return _prestamos.ObtenerPrestamosPendientes().Select(CrearPrestamoDto).ToList();
    }

    public List<PrestamoDto> ListarMisSolicitudes(string dniConsultante)
    {
        ObtenerUsuarioHabilitado(dniConsultante, Permiso.ConsultarMisSolicitudes, MensajeSinPermisoParaMisSolicitudes);

        return _prestamos.ObtenerPrestamosDeSolicitante(dniConsultante).Select(CrearPrestamoDto).ToList();
    }

    public List<PrestamoDto> ListarPrestamosActivos(string dniConsultante)
    {
        ObtenerUsuarioHabilitado(dniConsultante, Permiso.RealizarPrestamo, MensajeSinPermisoParaPrestamosActivos);
        DateTime ahora = _reloj.GetLocalNow().DateTime;

        return _prestamos.ObtenerPrestamosActivos()
            .OrderBy(prestamo => prestamo.FechaEsperadaDevolucion)
            .Select(prestamo => CrearPrestamoDtoConVencimiento(prestamo, ahora))
            .ToList();
    }

    public List<PrestamoDto> ListarPrestamos(string dniConsultante, FiltroDePrestamosDto filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        ObtenerUsuarioHabilitado(dniConsultante, Permiso.ConsultarPrestamos, MensajeSinPermisoParaConsultarPrestamos);
        ValidarRango(filtro);

        return _prestamos.ObtenerPrestamos(CrearFiltroDeDiasCompletos(filtro), ConvertirEstado(filtro.Estado))
            .OrderByDescending(prestamo => prestamo.FechaHoraPrestamo)
            .Select(CrearPrestamoDto)
            .ToList();
    }

    public FiltroDePrestamosDto RangoPorDefecto()
    {
        DateTime hoy = _reloj.GetLocalNow().Date;

        return new FiltroDePrestamosDto { Desde = hoy.AddDays(1 - DiasPorDefecto), Hasta = hoy };
    }

    private Usuario ObtenerSolicitante(string dniSolicitante)
    {
        Usuario? solicitante = _usuarios.ObtenerUsuarioPorDni(dniSolicitante);

        if (solicitante is null)
        {
            throw new ServicioException("El solicitante no existe");
        }

        if (!solicitante.EstaActivo())
        {
            throw new ServicioException("El solicitante esta desactivado");
        }

        if (!solicitante.TienePermiso(Permiso.SolicitarPrestamo))
        {
            throw new ServicioException("El solicitante no tiene permiso para solicitar prestamos");
        }

        return solicitante;
    }

    private Usuario ObtenerUsuarioHabilitado(string dni, Permiso permiso, string mensaje)
    {
        Usuario? usuario = _usuarios.ObtenerUsuarioPorDni(dni);

        if (usuario is null || !usuario.EstaHabilitadoPara(permiso))
        {
            throw new ServicioException(mensaje);
        }

        return usuario;
    }

    private Prestamo ObtenerPrestamo(Guid idPrestamo)
    {
        Prestamo? prestamo = _prestamos.ObtenerPrestamoPorId(idPrestamo);

        if (prestamo is null)
        {
            throw new ServicioException("El prestamo no existe");
        }

        return prestamo;
    }

    private TipoEquipamiento ObtenerTipoEquipamiento(int idCatalogo, string nombreTipo)
    {
        if (idCatalogo <= 0)
        {
            throw new ServicioException("Debe indicar un catalogo");
        }

        if (string.IsNullOrWhiteSpace(nombreTipo))
        {
            throw new ServicioException("Debe indicar un tipo de equipamiento");
        }

        TipoEquipamiento? tipo = _catalogo.ObtenerTipoPorNombre(idCatalogo, nombreTipo);

        if (tipo is null)
        {
            throw new ServicioException("El tipo de equipamiento no existe");
        }

        if (!tipo.EstaActivo)
        {
            throw new ServicioException("El tipo de equipamiento esta discontinuado");
        }

        return tipo;
    }

    private void ValidarLimites(Usuario solicitante, TipoEquipamiento tipo, int cantidadSolicitada)
    {
        if (!PermiteUnidadesEnElCatalogo(solicitante, tipo.Id, cantidadSolicitada))
        {
            throw new ServicioException("La cantidad solicitada supera el maximo de unidades del solicitante");
        }

        if (!PuedeTenerOtroPrestamoVigente(solicitante, idExcluido: null))
        {
            throw new ServicioException("El solicitante alcanzo el maximo de prestamos vigentes");
        }
    }

    private bool PermiteUnidadesEnElCatalogo(Usuario solicitante, int idTipoEquipamiento, int cantidadSolicitada)
    {
        int unidadesActivasEnElCatalogo =
            _prestamos.ContarUnidadesActivasDeSolicitanteEnCatalogoDelTipo(solicitante.Dni, idTipoEquipamiento);

        return solicitante.PermiteUnidades(unidadesActivasEnElCatalogo + cantidadSolicitada);
    }

    // idExcluido es el prestamo que se aprueba: ya esta Solicitado y no tiene que contarse dos veces.
    private bool PuedeTenerOtroPrestamoVigente(Usuario solicitante, Guid? idExcluido)
    {
        int prestamosVigentes = _prestamos.ContarPrestamosVigentesDeSolicitante(solicitante.Dni, idExcluido);

        return solicitante.PuedeAgregarPrestamo(prestamosVigentes);
    }

    private Prestamo RegistrarSolicitud(Usuario solicitante, TipoEquipamiento tipo, int cantidadSolicitada)
    {
        Prestamo prestamo = new Prestamo(
            solicitante,
            tipo,
            cantidadSolicitada,
            _reloj.GetLocalNow().DateTime);

        if (!tipo.RequiereAprobacion)
        {
            prestamo.ActivarORechazarPorFaltaDeUnidades();
        }

        _prestamos.AgregarPrestamo(prestamo);
        _movimientos.AgregarMovimiento(Movimiento.DeSolicitud(prestamo,
            RegistroDeAccion.DeUsuario(solicitante, prestamo.FechaHoraPrestamo)));

        Func<RegistroDeAccion> crearRegistroDelSistema =
            () => RegistroDeAccion.DelSistema(prestamo.FechaHoraPrestamo);

        if (prestamo.Estado == EstadoPrestamo.Rechazado)
        {
            _movimientos.AgregarMovimiento(Movimiento.DeRechazo(prestamo, crearRegistroDelSistema()));
        }

        RegistrarMovimientosDePrestamo(prestamo, crearRegistroDelSistema);

        _auditoria.Registrar(RegistroDeAccion.DeUsuario(solicitante, prestamo.FechaHoraPrestamo),
            AccionAuditada.SolicitudDePrestamo,
            $"Solicitud de préstamo de {solicitante.NombreCompletoConDni}. Unidades: {prestamo.CantidadSolicitada}");

        if (prestamo.Estado == EstadoPrestamo.Activo)
        {
            _auditoria.Registrar(crearRegistroDelSistema(), AccionAuditada.ActivacionDePrestamo,
                $"Activación del préstamo de {solicitante.NombreCompletoConDni}. Unidades: {prestamo.CantidadSolicitada}");
        }
        else if (prestamo.Estado == EstadoPrestamo.Rechazado)
        {
            _auditoria.Registrar(crearRegistroDelSistema(), AccionAuditada.RechazoDePrestamo,
                $"Rechazo del préstamo de {solicitante.NombreCompletoConDni}. Unidades: {prestamo.CantidadSolicitada}");
        }

        return prestamo;
    }

    private void ResolverAprobacion(Prestamo prestamo, Usuario solicitante, Usuario evaluador)
    {
        DateTime fechaHoraEvaluacion = _reloj.GetLocalNow().DateTime;

        DecidirEvaluacion(prestamo, solicitante, evaluador);
        _prestamos.ActualizarPrestamo(prestamo);
        RegistrarMovimientosDeEvaluacion(prestamo, evaluador, fechaHoraEvaluacion);
        _servicioNotificacion.RegistrarResultadoDeEvaluacion(prestamo, fechaHoraEvaluacion);
    }

    private void DecidirEvaluacion(Prestamo prestamo, Usuario solicitante, Usuario evaluador)
    {
        if (!PermiteUnidadesEnElCatalogo(solicitante, prestamo.TipoEquipamiento.Id, prestamo.CantidadSolicitada))
        {
            prestamo.RechazarPorTopeDeUnidades();
        }
        else if (!PuedeTenerOtroPrestamoVigente(solicitante, prestamo.Id))
        {
            prestamo.RechazarPorTopeDePrestamosConcurrentes();
        }
        else
        {
            prestamo.AprobarORechazarPorFaltaDeUnidades(evaluador);
        }
    }

    private void RegistrarMovimientosDeEvaluacion(Prestamo prestamo, Usuario evaluador, DateTime fechaHoraEvaluacion)
    {
        if (prestamo.Estado == EstadoPrestamo.Rechazado)
        {
            _movimientos.AgregarMovimiento(Movimiento.DeRechazo(prestamo,
                RegistroDeAccion.DelSistema(fechaHoraEvaluacion)));
        }
        else
        {
            Func<RegistroDeAccion> crearRegistroDelEvaluador =
                () => RegistroDeAccion.DeUsuario(evaluador, fechaHoraEvaluacion);

            foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
            {
                _movimientos.AgregarMovimiento(
                    Movimiento.DeAprobacion(prestamo, crearRegistroDelEvaluador(), unidad));
            }

            RegistrarMovimientosDePrestamo(prestamo, crearRegistroDelEvaluador);
        }
    }

    private void RegistrarRechazo(Prestamo prestamo, Usuario evaluador, string motivo)
    {
        DateTime fechaHoraEvaluacion = _reloj.GetLocalNow().DateTime;

        prestamo.Rechazar(evaluador, motivo);
        _prestamos.ActualizarPrestamo(prestamo);
        _movimientos.AgregarMovimiento(Movimiento.DeRechazo(prestamo,
            RegistroDeAccion.DeUsuario(evaluador, fechaHoraEvaluacion)));
        _servicioNotificacion.RegistrarResultadoDeEvaluacion(prestamo, fechaHoraEvaluacion);
    }

    // Un registro nuevo por movimiento: con EF, RegistroDeAccion es owned de Movimiento y una misma
    // instancia no puede pertenecer a dos movimientos.
    private void RegistrarMovimientosDePrestamo(Prestamo prestamo, Func<RegistroDeAccion> crearRegistro)
    {
        foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
        {
            _movimientos.AgregarMovimiento(Movimiento.DePrestamo(prestamo, crearRegistro(), unidad));
        }
    }

    private static void ValidarRango(FiltroDePrestamosDto filtro)
    {
        if (filtro.Desde > filtro.Hasta)
        {
            throw new ServicioException(nameof(FiltroDePrestamosDto.Desde), MensajeRangoInvertido);
        }
    }

    private static FiltroPorCatalogoYFechas CrearFiltroDeDiasCompletos(FiltroDePrestamosDto filtro)
    {
        return new FiltroPorCatalogoYFechas
        {
            Desde = filtro.Desde.Date,
            Hasta = InicioDelDiaSiguiente(filtro.Hasta)
        };
    }

    // El ultimo dia que DateTime puede representar no tiene dia siguiente: se toma hasta el ultimo instante.
    private static DateTime InicioDelDiaSiguiente(DateTime dia)
    {
        if (dia.Date == DateTime.MaxValue.Date)
        {
            return DateTime.MaxValue;
        }

        return dia.Date.AddDays(1);
    }

    private static EstadoPrestamo? ConvertirEstado(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            return null;
        }

        if (!Enum.GetNames<EstadoPrestamo>().Contains(estado))
        {
            throw new ServicioException(MensajeEstadoNoValido);
        }

        return Enum.Parse<EstadoPrestamo>(estado);
    }

    private static PrestamoDto CrearPrestamoDto(Prestamo prestamo)
    {
        return new PrestamoDto
        {
            Id = prestamo.Id,
            Solicitante = prestamo.Solicitante.NombreCompleto,
            TipoEquipamiento = prestamo.TipoEquipamiento.Nombre,
            CantidadSolicitada = prestamo.CantidadSolicitada,
            FechaHoraPrestamo = prestamo.FechaHoraPrestamo,
            FechaEsperadaDevolucion = prestamo.FechaEsperadaDevolucion,
            Estado = prestamo.Estado.ToString(),
            MotivoRechazo = prestamo.MotivoRechazo,
            Unidades = prestamo.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList()
        };
    }

    private static PrestamoDto CrearPrestamoDtoConVencimiento(Prestamo prestamo, DateTime ahora)
    {
        PrestamoDto prestamoDto = CrearPrestamoDto(prestamo);
        prestamoDto.EstaVencido = prestamo.EstaVencido(ahora);

        return prestamoDto;
    }
}