using Dominio;

namespace Servicios;

// Como se muestra un usuario en los listados: "Nombre Apellido".
internal static class NombresDeUsuario
{
    public static string Completo(Usuario usuario)
    {
        return $"{usuario.Nombre} {usuario.Apellido}";
    }
}
