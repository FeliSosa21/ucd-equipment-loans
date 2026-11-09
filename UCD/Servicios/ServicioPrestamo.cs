using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioPrestamo
{
    private const string MensajeSinPermisoParaEvaluar = "El usuario no tiene permiso para evaluar solicitudes";

    private readonly RepositorioPrestamoSql _prestamos;
    private readonly RepositorioUsuarioSql _usuarios;
    private readonly RepositorioCatalogoSql _catalogo;
    private readonly RepositorioMovimientoSql _movimientos;
    private readonly TimeProvider _reloj;

    public ServicioPrestamo(RepositorioPrestamoSql prestamos, RepositorioUsuarioSql usuarios,
        RepositorioCatalogoSql catalogo, RepositorioMovimientoSql movimientos, TimeProvider reloj)
    {
        _prestamos = prestamos;
        _usuarios = usuarios;
        _catalogo = catalogo;
        _movimientos = movimientos;
        _reloj = reloj;
    }

    public PrestamoDto Solicitar(SolicitudPrestamoDto datos, string dniSolicitante)
    {
        ArgumentNullException.ThrowIfNull(datos);

        Usuario solicitante = ObtenerSolicitante(dniSolicitante);
        TipoEquipamiento tipo = ObtenerTipoEquipamiento(datos.IdCatalogo, datos.TipoEquipamiento);
        ValidarLimites(solicitante, datos.CantidadSolicitada);

        Prestamo prestamo = TraduccionDeErrores.Ejecutar(
            () => RegistrarSolicitud(solicitante, tipo, datos.CantidadSolicitada));

        return CrearPrestamoDto(prestamo);
    }

    public PrestamoDto Aprobar(Guid idPrestamo, string dniEvaluador)
    {
        Usuario evaluador = ObtenerUsuarioHabilitado(
                    dniEvaluador, Permiso.EvaluarSolicitud, MensajeSinPermisoParaEvaluar);
        Prestamo prestamo = ObtenerPrestamo(idPrestamo);

        ObtenerSolicitante(prestamo.Solicitante.Dni);

        TraduccionDeErrores.Ejecutar(() => ResolverAprobacion(prestamo, evaluador));

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

    public List<PrestamoDto> ListarSolicitudesPendientes()
    {
        return _prestamos.ObtenerPrestamosPendientes().Select(CrearPrestamoDto).ToList();
    }

    public List<PrestamoDto> ListarPrestamosDeSolicitante(string dniSolicitante)
    {
        return _prestamos.ObtenerPrestamosDeSolicitante(dniSolicitante).Select(CrearPrestamoDto).ToList();
    }

    public List<PrestamoDto> ListarPrestamosPorEntregar()
    {
        return _prestamos.ObtenerPrestamosActivos().Select(CrearPrestamoDto).ToList();
    }

    public List<PrestamoDto> ListarPrestamos()
    {
        return _prestamos.ObtenerPrestamos(new FiltroPorCatalogoYFechas()).Select(CrearPrestamoDto).ToList();
    }

    private Usuario ObtenerSolicitante(string dniSolicitante)
    {
        Usuario? solicitante = _usuarios.ObtenerUsuarioPorDni(dniSolicitante);

        if (solicitante is null)
        {
            throw new ServicioException("El solicitante no existe");
        }

        if (solicitante.Estado == EstadoUsuario.Desactivado)
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

    private void ValidarLimites(Usuario solicitante, int cantidadSolicitada)
    {
        IReadOnlyList<Prestamo> prestamosDelSolicitante = _prestamos.ObtenerPrestamosDeSolicitante(solicitante.Dni);

        int unidadesActivas = prestamosDelSolicitante
            .Where(prestamo => prestamo.Estado == EstadoPrestamo.Activo)
            .Sum(prestamo => prestamo.UnidadesAsignadas.Count);

        if (!solicitante.Tipo.PermiteUnidades(unidadesActivas + cantidadSolicitada))
        {
            throw new ServicioException("La cantidad solicitada supera el maximo de unidades del solicitante");
        }

        int prestamosVigentes = prestamosDelSolicitante.Count(prestamo => prestamo.EstaVigente());

        if (!solicitante.Tipo.PuedeAgregarPrestamo(prestamosVigentes))
        {
            throw new ServicioException("El solicitante alcanzo el maximo de prestamos vigentes");
        }
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
            if (tipo.CantidadDisponibles() < cantidadSolicitada)
            {
                prestamo.RechazarPorFaltaDeUnidades();
            }
            else
            {
                List<UnidadFisica> unidades = tipo.UnidadesDisponibles().Take(cantidadSolicitada).ToList();
                prestamo.Activar(unidades);
            }
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

        RegistrarMovimientos(prestamo, crearRegistroDelSistema);

        return prestamo;
    }

    private void ResolverAprobacion(Prestamo prestamo, Usuario evaluador)
    {
        DateTime fechaHoraEvaluacion = _reloj.GetLocalNow().DateTime;

        if (prestamo.TipoEquipamiento.CantidadDisponibles() < prestamo.CantidadSolicitada)
        {
            prestamo.RechazarPorFaltaDeUnidades();

            _movimientos.AgregarMovimiento(Movimiento.DeRechazo(prestamo,
                RegistroDeAccion.DelSistema(fechaHoraEvaluacion)));
        }
        else
        {
            List<UnidadFisica> unidades = prestamo.TipoEquipamiento.UnidadesDisponibles()
                .Take(prestamo.CantidadSolicitada).ToList();
            prestamo.Aprobar(evaluador, unidades);
            Func<RegistroDeAccion> crearRegistroDelEvaluador =
                () => RegistroDeAccion.DeUsuario(evaluador, fechaHoraEvaluacion);

            foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
            {
                _movimientos.AgregarMovimiento(
                    Movimiento.DeAprobacion(prestamo, crearRegistroDelEvaluador(), unidad));
            }

            RegistrarMovimientos(prestamo, crearRegistroDelEvaluador);
        }

        _prestamos.ActualizarPrestamo(prestamo);
    }

    private void RegistrarRechazo(Prestamo prestamo, Usuario evaluador, string motivo)
    {
        prestamo.Rechazar(evaluador, motivo);
        _prestamos.ActualizarPrestamo(prestamo);
        _movimientos.AgregarMovimiento(Movimiento.DeRechazo(prestamo,
            RegistroDeAccion.DeUsuario(evaluador, _reloj.GetLocalNow().DateTime)));
    }

    // Un registro nuevo por movimiento: con EF, RegistroDeAccion es owned de Movimiento y una misma
    // instancia no puede pertenecer a dos movimientos.
    private void RegistrarMovimientos(Prestamo prestamo, Func<RegistroDeAccion> crearRegistro)
    {
        foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
        {
            _movimientos.AgregarMovimiento(Movimiento.DePrestamo(prestamo, crearRegistro(), unidad));
        }
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
}