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

        TipoEquipamiento tipo = _catalogo.ObtenerTipoPorNombre(datos.TipoEquipamiento)!;

        Prestamo prestamo = new Prestamo(solicitante, tipo, datos.CantidadSolicitada, _reloj.GetLocalNow().DateTime);

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

        return new PrestamoDto
        {
            Estado = prestamo.Estado.ToString(),
            MotivoRechazo = prestamo.MotivoRechazo
        };
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
}