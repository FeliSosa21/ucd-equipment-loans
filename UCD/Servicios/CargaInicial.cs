using Data.Repositorios;
using Dominio;

namespace Servicios;

// Datos que el sistema necesita al arrancar: el usuario root (administrador).
public class CargaInicial
{
    public const string DniRoot = "11111111";
    public const string CorreoRoot = "root@ucd.edu.uy";
    public const string ContrasenaRoot = "Root2026";

    private readonly RepositorioUsuario _repositorio;

    public CargaInicial(RepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    public void CargarRoot()
    {
        if (_repositorio.ObtenerUsuarioPorCorreo(CorreoRoot) != null)
        {
            return;
        }

        Usuario root = new Usuario(dni: DniRoot, nombre: "Root", apellido: "Admin", correo: CorreoRoot,
            contrasena: ContrasenaRoot);
        root.AsignarTipo(TipoUsuario.Administrador);
        root.ExigirCambioDeContrasena();
        _repositorio.AgregarUsuario(root);
    }
}
