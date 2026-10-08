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
        List<CantidadesPorEstadoDto> filas = new List<CantidadesPorEstadoDto>();
        Catalogo? catalogo = _repositorioCatalogo.Obtener();
        if (catalogo is null)
        {
            return filas;
        }

        foreach (TipoEquipamiento tipo in catalogo.Tipos)
        {
            filas.Add(CrearFilaDeTipo(tipo));
        }

        return filas;
    }

    private static CantidadesPorEstadoDto CrearFilaDeTipo(TipoEquipamiento tipo)
    {
        return new CantidadesPorEstadoDto
        {
            Nombre = tipo.Nombre,
            Categoria = tipo.Categoria.ToString(),
            Disponibles = ContarEnEstado(tipo.Unidades, EstadoUnidadFisica.Disponible),
            Prestadas = ContarEnEstado(tipo.Unidades, EstadoUnidadFisica.Prestada),
            EnMantenimiento = ContarEnEstado(tipo.Unidades, EstadoUnidadFisica.EnMantenimiento),
            DadasDeBaja = ContarEnEstado(tipo.Unidades, EstadoUnidadFisica.DadaDeBaja),
            Total = tipo.Unidades.Count
        };
    }

    private static int ContarEnEstado(IEnumerable<UnidadFisica> unidades, EstadoUnidadFisica estado)
    {
        return unidades.Count(unidad => unidad.Estado == estado);
    }
}