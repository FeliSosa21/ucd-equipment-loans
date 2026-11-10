using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioGestionCatalogos
{
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

        _repositorioCatalogo.AgregarCatalogo(new Catalogo(nombreNormalizado));
    }

    public void Modificar(int idCatalogo, string nombre, string dniResponsable)
    {
    }

    public void Eliminar(int idCatalogo, string dniResponsable)
    {
    }
}
