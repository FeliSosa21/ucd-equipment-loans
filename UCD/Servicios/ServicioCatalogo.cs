using System.Reflection;
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

        if (filtro.MinimoDisponibles != null && filtro.MinimoDisponibles < 0)
        {
            throw new ServicioException("La cantidad minima de unidades disponibles no puede ser negativa");
        }

        var catalogo = _repositorioCatalogo.Obtener();

        if (catalogo == null)
        {
            throw new ServicioException("Todavia no hay un catalogo cargado");
        }

        List<TipoEquipamientoDto> resultado = new List<TipoEquipamientoDto>();
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
        List<UnidadFisicaDto> resultado = new List<UnidadFisicaDto>();
        foreach (var unidad in tipo.UnidadesDisponibles())
        {
            resultado.Add(new UnidadFisicaDto {IdInstitucional = unidad.IdInstitucional, UbiFisica = unidad.UbiFisica, NumSerie = unidad.NumSerie});
        }

        return resultado;
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
}