using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioPrestamo
{
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
            RegistrarMovimientos(prestamo);

            return CrearPrestamoDto(prestamo);
        }
        catch (DominioException excepcion)
        {
            throw new ServicioException(excepcion.Message, excepcion);
        }
    }

    public PrestamoDto Aprobar(Guid idPrestamo, string dniEvaluador)
    {
        Prestamo? prestamo = _prestamos.ObtenerPrestamoPorId(idPrestamo);
        Usuario? evaluador = _usuarios.ObtenerUsuarioPorDni(dniEvaluador);

        if (evaluador is null
            || evaluador.Estado == EstadoUsuario.Desactivado
            || !evaluador.Tipo.TienePermiso(Permiso.EvaluarSolicitud))
        {
            throw new ServicioException("El usuario no tiene permiso para evaluar solicitudes");
        }

        if (prestamo is null)
        {
            throw new ServicioException("El prestamo no existe");
        }

        try
        {
            if (prestamo.TipoEquipamiento.CantidadDisponibles() < prestamo.CantidadSolicitada)
            {
                prestamo.RechazarPorFaltaDeUnidades();
            }
            else
            {
                List<UnidadFisica> unidades = prestamo.TipoEquipamiento.UnidadesDisponibles()
                                .Take(prestamo.CantidadSolicitada).ToList();
                            prestamo.Aprobar(evaluador, unidades);
            }
            return CrearPrestamoDto(prestamo);
        }
        catch (DominioException excepcion)
        {
            throw new ServicioException(excepcion.Message, excepcion);
        }
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
        if (!solicitante.Tipo.PermiteUnidades(cantidadSolicitada))
        {
            throw new ServicioException("La cantidad solicitada supera el maximo de unidades por prestamo");
        }

        int prestamosVigentes = _prestamos.ObtenerPrestamosDeSolicitante(solicitante.Dni)
            .Count(prestamo => prestamo.EstaVigente());

        if (!solicitante.Tipo.PuedeAgregarPrestamo(prestamosVigentes))
        {
            throw new ServicioException("El solicitante alcanzo el maximo de prestamos vigentes");
        }
    }

    private void RegistrarMovimientos(Prestamo prestamo)
    {
        foreach (UnidadFisica unidad in prestamo.UnidadesAsignadas)
        {
            _movimientos.AgregarMovimiento(new Movimiento(
                fechaHora: prestamo.FechaHoraPrestamo,
                tipo: TipoMovimiento.Prestamo,
                solicitante: prestamo.Solicitante,
                evaluador: null,
                unidad: unidad));
        }
    }

    private static PrestamoDto CrearPrestamoDto(Prestamo prestamo)
    {
        return new PrestamoDto
        {
            Id = prestamo.Id,
            TipoEquipamiento = prestamo.TipoEquipamiento.Nombre,
            CantidadSolicitada = prestamo.CantidadSolicitada,
            FechaHoraPrestamo = prestamo.FechaHoraPrestamo,
            Estado = prestamo.Estado.ToString(),
            MotivoRechazo = prestamo.MotivoRechazo,
            Unidades = prestamo.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional).ToList()
        };
    }
}