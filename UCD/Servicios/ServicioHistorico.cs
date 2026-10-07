using Data.Repositorios;
using DTOs;

namespace Servicios;

public class ServicioHistorico
{
    private readonly RepositorioMovimiento _movimientos;

    public ServicioHistorico(RepositorioMovimiento movimientos)
    {
        _movimientos = movimientos;
    }

    public List<MovimientoDto> ListarMovimientos()
    {
        return new List<MovimientoDto>();
    }
}
