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
    // OrderByDescending es estable: los movimientos con la misma fecha quedan en el orden en que se registraron.
    public List<MovimientoDto> ListarMovimientos()
    {
        return _movimientos.ObtenerTodos()
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
