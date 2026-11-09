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

    // El responsable se valida en cada operacion: la pantalla pudo abrirse antes de que lo desactivaran.
    public static void ValidarResponsable(RepositorioUsuarioSql repositorioUsuario, string dniResponsable,
        Permiso permiso, string mensajeSinPermiso)
    {
        Usuario? responsable = repositorioUsuario.ObtenerUsuarioPorDni(dniResponsable);
        if (responsable is null || responsable.Estado == EstadoUsuario.Desactivado ||
            !responsable.Tipo.TienePermiso(permiso))
        {
            throw new ServicioException(mensajeSinPermiso);
        }
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
