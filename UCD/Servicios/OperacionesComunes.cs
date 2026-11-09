using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Servicios;

// Pasos que comparten los servicios del catalogo (consulta, inventario, tipos y unidades).
internal static class OperacionesComunes
{
    public static Catalogo ObtenerCatalogoActivo(RepositorioCatalogoSql repositorioCatalogo, int idCatalogo)
    {
        Catalogo? catalogo = repositorioCatalogo.ObtenerCatalogoActivoPorId(idCatalogo);
        if (catalogo is null)
        {
            throw new ServicioException("El catalogo no existe o fue dado de baja");
        }

        return catalogo;
    }

    // Una sola forma de obtener un tipo: por catalogo y nombre, sin importar mayusculas ni espacios.
    public static TipoEquipamiento ObtenerTipo(RepositorioCatalogoSql repositorioCatalogo, int idCatalogo,
        string nombre)
    {
        TipoEquipamiento? tipo = repositorioCatalogo.ObtenerTipoPorNombre(idCatalogo, nombre);
        if (tipo is null)
        {
            throw new ServicioException("El tipo de equipamiento no existe");
        }

        return tipo;
    }

    // Ultima barrera: si la base rechaza el guardado (por ejemplo un indice unico), la UI recibe un ServicioException.
    public static void GuardarCambios(RepositorioCatalogoSql repositorioCatalogo, Catalogo catalogo,
        string mensajeSiFalla)
    {
        try
        {
            repositorioCatalogo.ActualizarCatalogo(catalogo);
        }
        catch (DbUpdateException ex)
        {
            throw new ServicioException(mensajeSiFalla, ex);
        }
    }

    // El responsable se valida en cada operacion: la pantalla pudo abrirse antes de que lo desactivaran.
    public static Usuario ValidarResponsable(Usuario? responsable, Permiso permiso, string mensajeSinPermiso)
    {
        Usuario activo = ObtenerResponsableActivo(responsable, mensajeSinPermiso);
        if (!activo.TienePermiso(permiso))
        {
            throw new ServicioException(mensajeSinPermiso);
        }

        return activo;
    }

    public static Usuario ObtenerResponsableActivo(Usuario? responsable, string mensajeSinPermiso)
    {
        if (responsable is null || !responsable.EstaActivo())
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
