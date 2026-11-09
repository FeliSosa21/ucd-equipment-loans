using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioCatalogo
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para consultar el catalogo";

    private readonly RepositorioCatalogoSql _repositorioCatalogo;
    private readonly RepositorioUsuarioSql _repositorioUsuario;

    public ServicioCatalogo(RepositorioCatalogoSql repositorioCatalogo, RepositorioUsuarioSql repositorioUsuario)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
    }

    public List<CatalogoDto> ListarCatalogos(string dniUsuario)
    {
        ValidarResponsable(dniUsuario);
        var resultado = new List<CatalogoDto>();

        foreach (var catalogo in _repositorioCatalogo.ObtenerCatalogosActivos())
        {
            resultado.Add(new CatalogoDto { Id = catalogo.Id, Nombre = catalogo.Nombre });
        }

        return resultado;
    }

    public List<TipoEquipamientoDto> ConsultarTipos(int idCatalogo, FiltroCatalogoDto filtro, string dniUsuario)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        Usuario usuario = ValidarResponsable(dniUsuario);
        bool veDiscontinuados = usuario.TienePermiso(Permiso.GestionarCatalogo);
        ValidarFiltro(filtro);
        var catalogo = OperacionesComunes.ObtenerCatalogoActivo(_repositorioCatalogo, idCatalogo);
        var resultado = new List<TipoEquipamientoDto>();

        foreach (var tipo in catalogo.Tipos)
        {
            if (CumpleFiltro(tipo, filtro) && (veDiscontinuados || tipo.EstaActivo))
            {
                resultado.Add(CrearTipoDto(tipo));
            }
        }

        return resultado;
    }

    public List<UnidadFisicaDto> ObtenerUnidadesDisponibles(int idCatalogo, string nombreTipo, string dniUsuario)
    {
        ValidarResponsable(dniUsuario);
        var tipo = OperacionesComunes.ObtenerTipo(_repositorioCatalogo, idCatalogo, nombreTipo);

        var resultado = new List<UnidadFisicaDto>();

        foreach (var unidad in tipo.UnidadesDisponibles())
        {
            resultado.Add(CrearUnidadDto(unidad));
        }

        return resultado;
    }

    private Usuario ValidarResponsable(string dniUsuario)
    {
        return OperacionesComunes.ValidarResponsable(_repositorioUsuario, dniUsuario, Permiso.ConsultarCatalogo,
            MensajeSinPermiso);
    }

    private void ValidarFiltro (FiltroCatalogoDto filtro)
    {
        if (filtro.MinimoDisponibles != null && filtro.MinimoDisponibles < 0)
        {
            throw new ServicioException("La cantidad minima de unidades disponibles no puede ser negativa");
        }
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