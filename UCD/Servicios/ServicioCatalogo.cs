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
        var catalogo = _repositorioCatalogo.Obtener();
        List<TipoEquipamientoDto> listaTipoEquipamientos = new List<TipoEquipamientoDto>();
        foreach (var tipo in catalogo!.Tipos)
        {
            if (CumpleFiltro(tipo, filtro))
            {
                listaTipoEquipamientos.Add(new TipoEquipamientoDto { Nombre = tipo.Nombre });
            }
        }

        return listaTipoEquipamientos;
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
}