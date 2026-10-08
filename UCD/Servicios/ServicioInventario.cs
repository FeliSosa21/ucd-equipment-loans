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
            if (CumpleFiltro(tipo, filtro))
            {
                filas.Add(CrearFilaDeTipo(tipo));
            }
        }

        return filas;
    }

    public List<CantidadesPorEstadoDto> ContarPorCategoria()
    {
        List<CantidadesPorEstadoDto> filas = new List<CantidadesPorEstadoDto>();
        Catalogo? catalogo = _repositorioCatalogo.Obtener();
        foreach (Categoria categoria in Enum.GetValues<Categoria>())
        {
            List<UnidadFisica> unidades = new List<UnidadFisica>();
            if (catalogo is not null)
            {
                foreach (TipoEquipamiento tipo in catalogo.Tipos)
                {
                    if (tipo.Categoria == categoria)
                    {
                        unidades.AddRange(tipo.Unidades);
                    }
                }
            }

            filas.Add(new CantidadesPorEstadoDto
            {
                Nombre = categoria.ToString(),
                Categoria = categoria.ToString(),
                Disponibles = ContarEnEstado(unidades, EstadoUnidadFisica.Disponible),
                Prestadas = ContarEnEstado(unidades, EstadoUnidadFisica.Prestada),
                EnMantenimiento = ContarEnEstado(unidades, EstadoUnidadFisica.EnMantenimiento),
                DadasDeBaja = ContarEnEstado(unidades, EstadoUnidadFisica.DadaDeBaja),
                Total = unidades.Count
            });
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
        int cantidad = 0;
        foreach (UnidadFisica unidad in unidades)
        {
            if (unidad.Estado == estado)
            {
                cantidad++;
            }
        }

        return cantidad;
    }

    private static bool CumpleFiltro(TipoEquipamiento tipo, FiltroInventarioDto filtro)
    {
        if (filtro.Categoria is not null && tipo.Categoria.ToString() != filtro.Categoria)
        {
            return false;
        }

        if (filtro.Estado is not null && !TieneUnidadesEnEstado(tipo, filtro.Estado))
        {
            return false;
        }

        return true;
    }

    private static bool TieneUnidadesEnEstado(TipoEquipamiento tipo, string estado)
    {
        foreach (UnidadFisica unidad in tipo.Unidades)
        {
            if (unidad.Estado.ToString() == estado)
            {
                return true;
            }
        }

        return false;
    }
}