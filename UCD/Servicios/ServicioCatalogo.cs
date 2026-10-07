using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioCatalogo
{
    private readonly RepositorioCatalogo _repositorioCatalogo;

    public ServicioCatalogo(RepositorioCatalogo repositorioCatalogo)
    {
        _repositorioCatalogo = repositorioCatalogo;
    }

    public List<TipoEquipamientoDto> ConsultarTipos(FiltroCatalogoDto filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ValidarFiltro(filtro);
        var catalogo = ObtenerCatalogo();
        var resultado = new List<TipoEquipamientoDto>();

        foreach (var tipo in catalogo.Tipos)
        {
            if (CumpleFiltro(tipo, filtro))
            {
                resultado.Add(CrearTipoDto(tipo));
            }
        }

        return resultado;
    }

    public List<UnidadFisicaDto> ObtenerUnidadesDisponibles(string nombreTipo)
    {
        var tipo = _repositorioCatalogo.ObtenerTipoPorNombre(nombreTipo);

        if (tipo is null)
        {
            throw new ServicioException("No existe un tipo de equipamiento con ese nombre");
        }

        var resultado = new List<UnidadFisicaDto>();

        foreach (var unidad in tipo.UnidadesDisponibles())
        {
            resultado.Add(CrearUnidadDto(unidad));
        }

        return resultado;
    }

    private void ValidarFiltro (FiltroCatalogoDto filtro)
    {
        if (filtro.MinimoDisponibles != null && filtro.MinimoDisponibles < 0)
        {
            throw new ServicioException("La cantidad minima de unidades disponibles no puede ser negativa");
        }
    }

    private Catalogo ObtenerCatalogo()
    {
        var catalogo = _repositorioCatalogo.Obtener();

        if (catalogo is null)
        {
            throw new ServicioException("Todavia no hay un catalogo cargado");
        }

        return catalogo;
    }

    private static bool CumpleFiltro(TipoEquipamiento tipo, FiltroCatalogoDto filtro)
    {
        if (filtro.Categoria != null && tipo.Categoria.ToString() != filtro.Categoria)
        {
            return false;
        }

        if (filtro.Estado != null && tipo.Estado.ToString() != filtro.Estado)
        {
            return false;
        }

        if (filtro.MinimoDisponibles != null && tipo.CantidadDisponibles() < filtro.MinimoDisponibles)
        {
            return false;
        }

        return true;
    }

    private static TipoEquipamientoDto CrearTipoDto(TipoEquipamiento tipo)
    {
        return new TipoEquipamientoDto
        {
            Nombre = tipo.Nombre,
            Categoria = tipo.Categoria.ToString(),
            Estado = tipo.Estado.ToString(),
            CantidadDisponibles = tipo.CantidadDisponibles()
        };
    }

    private static UnidadFisicaDto CrearUnidadDto(UnidadFisica unidad)
    {
        return new UnidadFisicaDto
        {
            IdInstitucional = unidad.IdInstitucional,
            UbiFisica = unidad.UbiFisica,
            NumSerie = unidad.NumSerie
        };
    }
}