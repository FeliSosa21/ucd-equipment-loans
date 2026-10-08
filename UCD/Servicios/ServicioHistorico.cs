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
    // Del mas reciente al mas viejo; con la misma fecha va primero el ultimo registrado: se invierte el orden
    // del repositorio y OrderByDescending, que es estable, lo respeta en los empates.
    public List<MovimientoDto> ListarMovimientos()
    {
        return Enumerable.Reverse(_movimientos.ObtenerTodos())
            .OrderByDescending(movimiento => movimiento.FechaHora)
            .Select(CrearMovimientoDto)
            .ToList();
    }

    private static MovimientoDto CrearMovimientoDto(Movimiento movimiento)
    {
        return new MovimientoDto
        {
            FechaHora = movimiento.FechaHora,
            Tipo = movimiento.Tipo.ToString(),
            Solicitante = NombresDeUsuario.Completo(movimiento.Solicitante),
            Evaluador = movimiento.Evaluador is null ? null : NombresDeUsuario.Completo(movimiento.Evaluador),
            Unidad = movimiento.Unidad?.IdInstitucional
        };
    }
}
