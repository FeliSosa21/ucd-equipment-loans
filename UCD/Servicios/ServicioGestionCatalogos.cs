using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Servicios;

public class ServicioGestionCatalogos
{
    private const string CampoNombre = nameof(CatalogoDto.Nombre);
    private const string MensajeSinPermiso = "El usuario no tiene permiso para gestionar los catalogos";

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
        ValidarResponsable(dniResponsable);
        string nombreNormalizado = OperacionesComunes.Normalizar(nombre);
        if (_repositorioCatalogo.ExisteActivoConNombre(nombreNormalizado, null))
        {
            throw new ServicioException(CampoNombre, "Ya existe un catalogo con ese nombre");
        }

        Catalogo catalogo = TraduccionDeErrores.Ejecutar(() => new Catalogo(nombreNormalizado), CampoNombre);
        AgregarCatalogo(catalogo);
    }

    public void Modificar(int idCatalogo, string nombre, string dniResponsable)
    {
    }

    public void Eliminar(int idCatalogo, string dniResponsable)
    {
    }

    // Dos usuarios pueden crear el mismo nombre a la vez: el indice unico de la base rechaza al segundo.
    private void AgregarCatalogo(Catalogo catalogo)
    {
        try
        {
            _repositorioCatalogo.AgregarCatalogo(catalogo);
        }
        catch (DbUpdateException ex)
        {
            throw new ServicioException(CampoNombre, "No se pudo crear el catalogo: ya existe uno con ese nombre", ex);
        }
    }

    private void ValidarResponsable(string dniResponsable)
    {
        OperacionesComunes.ValidarResponsable(_repositorioUsuario.ObtenerUsuarioPorDni(dniResponsable),
            Permiso.GestionarCatalogo, MensajeSinPermiso);
    }
}
