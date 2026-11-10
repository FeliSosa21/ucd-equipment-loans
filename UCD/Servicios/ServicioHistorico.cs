using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioHistorico
{
    private readonly RepositorioMovimientoSql _movimientos;

    public ServicioHistorico(RepositorioMovimientoSql movimientos)
    {
        _movimientos = movimientos;
    }

    // Temporal, hasta que la pagina filtre por catalogo: no recibe quien consulta (el permiso lo controla la pagina).
    // El repositorio no devuelve nada sin criterios, asi que Desde = MinValue pide todo lo registrado.
    public List<MovimientoDto> ListarMovimientos()
    {
        FiltroPorCatalogoYFechas desdeSiempre = new FiltroPorCatalogoYFechas { Desde = DateTime.MinValue };

        return _movimientos.ObtenerMovimientos(desdeSiempre)
            .Select(CrearMovimientoDto)
            .ToList();
    }

    private static MovimientoDto CrearMovimientoDto(Movimiento movimiento)
    {
        return new MovimientoDto
        {
            FechaHora = movimiento.FechaHora,
            Tipo = movimiento.Tipo.ToString(),
            Solicitante = movimiento.Solicitante.NombreCompleto,
            Evaluador = movimiento.Evaluador?.NombreCompleto,
            Responsable = movimiento.Responsable?.NombreCompleto ?? "Sistema",
            Unidad = movimiento.Unidad?.IdInstitucional
        };
    }
}
