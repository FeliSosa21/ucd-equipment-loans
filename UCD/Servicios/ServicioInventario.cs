using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Consulta de inventario (req. 9): cantidad de unidades en cada estado, por tipo y por categoria.
// La disponibilidad sale del estado de las unidades fisicas (letra).
// Como el historico, no recibe quien consulta: el permiso lo controla la pagina.
public class ServicioInventario
{
    private readonly RepositorioCatalogo _repositorioCatalogo;

    public ServicioInventario(RepositorioCatalogo repositorioCatalogo)
    {
        _repositorioCatalogo = repositorioCatalogo;
    }

    public List<CantidadesPorEstadoDto> ContarPorTipo(FiltroInventarioDto filtro)
    {
        return new List<CantidadesPorEstadoDto>();
    }
}