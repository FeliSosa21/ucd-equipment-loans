using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioGestionCatalogos
{
    private const string CampoNombre = nameof(CatalogoDto.Nombre);

    private readonly RepositorioCatalogoSql _repositorioCatalogo;
    private readonly RepositorioUsuarioSql _repositorioUsuario;
    private readonly RepositorioPrestamoSql _repositorioPrestamo;

    public ServicioGestionCatalogos(RepositorioCatalogoSql repositorioCatalogo,
        RepositorioUsuarioSql repositorioUsuario, RepositorioPrestamoSql repositorioPrestamo)
    {
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioUsuario = repositorioUsuario;
        _repositorioPrestamo = repositorioPrestamo;
    }

    public List<CatalogoDto> Listar(string dniUsuario)
    {
        return new List<CatalogoDto>();
    }

    public void Crear(string nombre, string dniResponsable)
    {
        string nombreNormalizado = OperacionesComunes.Normalizar(nombre);
        if (_repositorioCatalogo.ExisteActivoConNombre(nombreNormalizado, null))
        {
            throw new ServicioException(CampoNombre, "Ya existe un catalogo con ese nombre");
        }

        _repositorioCatalogo.AgregarCatalogo(new Catalogo(nombreNormalizado));
    }

    public void Modificar(int idCatalogo, string nombre, string dniResponsable)
    {
    }

    public void Eliminar(int idCatalogo, string dniResponsable)
    {
    }
}
