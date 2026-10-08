using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioPrestamo
{
    private const string MensajeSinPermisoParaEvaluar = "El usuario no tiene permiso para evaluar solicitudes";
    private const string MensajeSinPermisoParaRealizar = "El usuario no tiene permiso para realizar prestamos";

    private readonly RepositorioPrestamo _prestamos;
    private readonly RepositorioUsuario _usuarios;
    private readonly RepositorioCatalogo _catalogo;
    private readonly RepositorioMovimiento _movimientos;
    private readonly TimeProvider _reloj;

    public ServicioPrestamo(RepositorioPrestamo prestamos, RepositorioUsuario usuarios,
        RepositorioCatalogo catalogo, RepositorioMovimiento movimientos, TimeProvider reloj)
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
        TipoEquipamiento tipo = ObtenerTipoEquipamiento(datos.TipoEquipamiento);
        ValidarLimites(solicitante, datos.CantidadSolicitada);

        try
        {
            Prestamo prestamo = new Prestamo(
                solicitante,
                tipo,
                datos.CantidadSolicitada,
                _reloj.GetLocalNow().DateTime);

            if (!tipo.RequiereAprobacion)
            {
                if (tipo.CantidadDisponibles() < datos.CantidadSolicitada)
                {
                    prestamo.RechazarPorFaltaDeUnidades();
                }
                else
                {
                    List<UnidadFisica> unidades = tipo.UnidadesDisponibles().Take(datos.CantidadSolicitada).ToList();
                    prestamo.Activar(unidades);
                }
            }

            _prestamos.AgregarPrestamo(prestamo);
            _movimientos.AgregarMovimiento(new Movimiento(
                fechaHora: prestamo.FechaHoraPrestamo,
                tipo: TipoMovimiento.Solicitud,
                solicitante: prestamo.Solicitante,
                evaluador: null,
                unidad: null));

            if (prestamo.Estado == EstadoPrestamo.Rechazado)
            {
                _movimientos.AgregarMovimiento(new Movimiento(
                    fechaHora: prestamo.FechaHoraPrestamo,
                    tipo: TipoMovimiento.Rechazo,
                    solicitante: prestamo.Solicitante,
                    evaluador: null,
                    unidad: null));
            }

            RegistrarMovimientos(prestamo, prestamo.FechaHoraPrestamo);

            return CrearPrestamoDto(prestamo);
        }
        catch (DominioException excepcion)
        {
            throw new ServicioException(excepcion.Message, excepcion);
        }
    }

    public PrestamoDto Aprobar(Guid idPrestamo, string dniEvaluador)
    {
        Usuario evaluador = ObtenerUsuarioHabilitado(
                    dniEvaluador, Permiso.EvaluarSolicitud, MensajeSinPermisoParaEvaluar);
        Prestamo prestamo = ObtenerPrestamo(idPrestamo);

        ObtenerSolicitante(prestamo.Solicitante.Dni);

        try
        {
            DateTime fechaHoraEvaluacion = _reloj.GetLocalNow().DateTime;

            if (prestamo.TipoEquipamiento.CantidadDisponibles() < prestamo.CantidadSolicitada)
            {
                prestamo.RechazarPorFaltaDeUnidades();

                _movimientos.AgregarMovimiento(new Movimiento(
                    fechaHora: fechaHoraEvaluacion,
                    tipo: TipoMovimiento.Rechazo,
                    solicitante: prestamo.Solicitante,
                    evaluador: null,
                    unidad: null));
            }
            else
            {
                List<UnidadFisica> unidades = prestamo.TipoEquipamiento.UnidadesDisponibles()
                    .Take(prestamo.CantidadSolicitada).ToList();
                prestamo.Aprobar(evaluador, unidades);

                foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
                {
                    _movimientos.AgregarMovimiento(new Movimiento(
                        fechaHora: fechaHoraEvaluacion,
                        tipo: TipoMovimiento.Aprobacion,
                        solicitante: prestamo.Solicitante,
                        evaluador: evaluador,
                        unidad: unidad));
                }

                prestamo.Realizar();
                RegistrarMovimientos(prestamo, fechaHoraEvaluacion);
            }

            return CrearPrestamoDto(prestamo);
        }
        catch (DominioException excepcion)
        {
            throw new ServicioException(excepcion.Message, excepcion);
        }
    }

    public PrestamoDto Rechazar(Guid idPrestamo, string motivo, string dniEvaluador)
    {
        Usuario evaluador = ObtenerUsuarioHabilitado(
                    dniEvaluador, Permiso.EvaluarSolicitud, MensajeSinPermisoParaEvaluar);
        Prestamo prestamo = ObtenerPrestamo(idPrestamo);

        try
        {
            prestamo.Rechazar(evaluador, motivo);

            return CrearPrestamoDto(prestamo);
        }
        catch (DominioException excepcion)
        {
            throw new ServicioException(excepcion.Message, excepcion);
        }
    }

    public PrestamoDto RegistrarEntrega(Guid idPrestamo, string dniResponsable)
    {
        Usuario responsable = ObtenerUsuarioHabilitado(
            dniResponsable, Permiso.RealizarPrestamo, MensajeSinPermisoParaRealizar);
        Prestamo prestamo = ObtenerPrestamo(idPrestamo);

        try
        {
            prestamo.RegistrarEntrega(responsable, _reloj.GetLocalNow().DateTime);

            return CrearPrestamoDto(prestamo);
        }
        catch (DominioException excepcion)
        {
            throw new ServicioException(excepcion.Message, excepcion);
        }
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
        return _prestamos.ObtenerPrestamosActivos()
            .Where(prestamo => prestamo.FechaHoraEntrega is null)
            .Select(CrearPrestamoDto)
            .ToList();
    }

    public List<PrestamoDto> ListarPrestamos()
    {
        return _prestamos.ObtenerTodos().Select(CrearPrestamoDto).ToList();
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

        if (!solicitante.Tipo.TienePermiso(Permiso.SolicitarPrestamo))
        {
            throw new ServicioException("El solicitante no tiene permiso para solicitar prestamos");
        }

        return solicitante;
    }

    private Usuario ObtenerUsuarioHabilitado(string dni, Permiso permiso, string mensaje)
    {
        Usuario? usuario = _usuarios.ObtenerUsuarioPorDni(dni);

        if (usuario is null
            || usuario.Estado == EstadoUsuario.Desactivado
            || !usuario.Tipo.TienePermiso(permiso))
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

    private TipoEquipamiento ObtenerTipoEquipamiento(string nombreTipo)
    {
        if (string.IsNullOrWhiteSpace(nombreTipo))
        {
            throw new ServicioException("Debe indicar un tipo de equipamiento");
        }

        TipoEquipamiento? tipo = _catalogo.ObtenerTipoPorNombre(nombreTipo);

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

    private void RegistrarMovimientos(Prestamo prestamo, DateTime fechaHora)
    {
        foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
        {
            _movimientos.AgregarMovimiento(new Movimiento(
                fechaHora: fechaHora,
                tipo: TipoMovimiento.Prestamo,
                solicitante: prestamo.Solicitante,
                evaluador: prestamo.Evaluador,
                unidad: unidad));
        }
    }

    private static PrestamoDto CrearPrestamoDto(Prestamo prestamo)
    {
        return new PrestamoDto
        {
            Id = prestamo.Id,
            Solicitante = $"{prestamo.Solicitante.Nombre} {prestamo.Solicitante.Apellido}",
            TipoEquipamiento = prestamo.TipoEquipamiento.Nombre,
            CantidadSolicitada = prestamo.CantidadSolicitada,
            FechaHoraPrestamo = prestamo.FechaHoraPrestamo,
            FechaEsperadaDevolucion = prestamo.FechaEsperadaDevolucion,
            Estado = prestamo.Estado.ToString(),
            MotivoRechazo = prestamo.MotivoRechazo,
            Unidades = prestamo.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList(),
            FechaHoraEntrega = prestamo.FechaHoraEntrega
        };
    }
}