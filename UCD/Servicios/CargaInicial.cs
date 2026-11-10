using Data.Repositorios;
using Dominio;

namespace Servicios;

// Datos que el sistema necesita al arrancar: el usuario root (administrador).
public class CargaInicial
{
    private const string NombreTipoAdministrador = "Administrador";

    public const string DniRoot = CargaInicialSql.DniRoot;
    public const string CorreoRoot = CargaInicialSql.CorreoRoot;
    public const string ContrasenaRoot = CargaInicialSql.ContrasenaRoot;
    public const string NombreCatalogo = "Catalogo UCD";

    private readonly RepositorioUsuario _repositorioUsuario;
    private readonly RepositorioCatalogo _repositorioCatalogo;
    private readonly RepositorioTipoUsuario _repositorioTipoUsuario;

    public CargaInicial(RepositorioUsuario repositorioUsuario, RepositorioCatalogo repositorioCatalogo,
        RepositorioTipoUsuario repositorioTipoUsuario)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioCatalogo = repositorioCatalogo;
        _repositorioTipoUsuario = repositorioTipoUsuario;
    }

    public void CargarTiposDeUsuario()
    {
        foreach (TipoUsuario tipo in TipoUsuario.Todos())
        {
            _repositorioTipoUsuario.AgregarTipo(tipo);
        }
    }

    public void CargarRoot()
    {
        if (_repositorioUsuario.ObtenerUsuarioPorDni(DniRoot) != null)
        {
            return;
        }

        Usuario root = new Usuario(dni: DniRoot, nombre: "Root", apellido: "Admin", correo: CorreoRoot,
            contrasena: ContrasenaRoot);
        root.AsignarTipo(_repositorioTipoUsuario.ObtenerTipoPorNombre(NombreTipoAdministrador)!);
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
