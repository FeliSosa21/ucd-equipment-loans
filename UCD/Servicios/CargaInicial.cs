using Data.Repositorios;
using Dominio;

namespace Servicios;

// Datos que el sistema necesita al arrancar: el usuario root (administrador).
public class CargaInicial
{
    public const string DniRoot = "11111111";
    public const string CorreoRoot = "root@ucd.edu.uy";
    public const string ContrasenaRoot = "Root2026";
    public const string NombreCatalogo = "Catalogo UCD";

    private readonly RepositorioUsuario _repositorioUsuario;
    private readonly RepositorioCatalogo _repositorioCatalogo;

    public CargaInicial(RepositorioUsuario repositorioUsuario, RepositorioCatalogo repositorioCatalogo)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioCatalogo = repositorioCatalogo;
    }

    public void CargarRoot()
    {
        if (_repositorioUsuario.ObtenerUsuarioPorCorreo(CorreoRoot) != null)
        {
            return;
        }

        Usuario root = new Usuario(dni: DniRoot, nombre: "Root", apellido: "Admin", correo: CorreoRoot,
            contrasena: ContrasenaRoot);
        root.AsignarTipo(TipoUsuario.Administrador);
        // El root no debe cambiar la contrasena en el primer ingreso: la letra lo exceptua ("salvo el root").
        _repositorioUsuario.AgregarUsuario(root);
    }

    public void CargarCatalogo()
    {
        if (_repositorioCatalogo.Obtener() != null)
        {
            return;
        }

        _repositorioCatalogo.Guardar(new Catalogo(NombreCatalogo));
    }
}
