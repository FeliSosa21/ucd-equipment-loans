using Data.Repositorios;
using Dominio;

namespace Servicios;

// Pasos que comparten los servicios de gestion del catalogo (tipos, unidades e inventario).
internal static class OperacionesComunes
{
    public static Catalogo ObtenerCatalogo(RepositorioCatalogo repositorioCatalogo)
    {
        Catalogo? catalogo = repositorioCatalogo.Obtener();
        if (catalogo is null)
        {
            throw new ServicioException("El catalogo no esta cargado");
        }

        return catalogo;
    }

    public static Catalogo ObtenerCatalogoActivo(RepositorioCatalogoSql repositorioCatalogo, int idCatalogo)
    {
        Catalogo? catalogo = repositorioCatalogo.ObtenerCatalogoActivoPorId(idCatalogo);
        if (catalogo is null)
        {
            throw new ServicioException("El catalogo no existe o fue dado de baja");
        }

        return catalogo;
    }

    // El responsable se valida en cada operacion: la pantalla pudo abrirse antes de que lo desactivaran.
    public static Usuario ValidarResponsable(RepositorioUsuarioSql repositorioUsuario, string dniResponsable,
        Permiso permiso, string mensajeSinPermiso)
    {
        Usuario responsable = ObtenerResponsableActivo(repositorioUsuario, dniResponsable, mensajeSinPermiso);
        if (!responsable.Tipo.TienePermiso(permiso))
        {
            throw new ServicioException(mensajeSinPermiso);
        }

        return responsable;
    }

    public static Usuario ObtenerResponsableActivo(RepositorioUsuarioSql repositorioUsuario, string dniResponsable,
        string mensajeSinPermiso)
    {
        Usuario? responsable = repositorioUsuario.ObtenerUsuarioPorDni(dniResponsable);
        if (responsable is null || responsable.Estado == EstadoUsuario.Desactivado)
        {
            throw new ServicioException(mensajeSinPermiso);
        }

        return responsable;
    }

    public static string Normalizar(string? texto)
    {
        if (texto is null)
        {
            return string.Empty;
        }

        return texto.Trim();
    }
}
