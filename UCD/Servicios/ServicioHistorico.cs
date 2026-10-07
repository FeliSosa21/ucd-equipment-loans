using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioHistorico
{
    private readonly RepositorioMovimiento _movimientos;

    public ServicioHistorico(RepositorioMovimiento movimientos)
    {
        _movimientos = movimientos;
    }

    // No recibe quien consulta: el permiso lo controla la pagina, como en ListarSolicitudesPendientes.
    public List<MovimientoDto> ListarMovimientos()
    {
        return _movimientos.ObtenerTodos().Select(CrearMovimientoDto).ToList();
    }

    private static MovimientoDto CrearMovimientoDto(Movimiento movimiento)
    {
        return new MovimientoDto
        {
            FechaHora = movimiento.FechaHora,
            Tipo = movimiento.Tipo.ToString(),
            Solicitante = NombreCompleto(movimiento.Solicitante),
            Evaluador = movimiento.Evaluador is null ? null : NombreCompleto(movimiento.Evaluador),
            Unidad = movimiento.Unidad?.IdInstitucional
        };
    }

    private static string NombreCompleto(Usuario usuario)
    {
        return $"{usuario.Nombre} {usuario.Apellido}";
    }
}
