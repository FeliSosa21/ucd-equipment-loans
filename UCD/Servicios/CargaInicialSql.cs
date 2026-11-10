using Data.Repositorios;
using Dominio;

namespace Servicios;

public class CargaInicialSql
{
    private const string NombreTipoAdministrador = "Administrador";

    private readonly RepositorioUsuarioSql _repositorioUsuario;
    private readonly RepositorioTipoUsuarioSql _repositorioTipoUsuario;

    public CargaInicialSql(RepositorioUsuarioSql repositorioUsuario, RepositorioTipoUsuarioSql repositorioTipoUsuario)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioTipoUsuario = repositorioTipoUsuario;
    }

    public void CargarRoot()
    {
        if (_repositorioUsuario.ObtenerUsuarioPorDni(CargaInicial.DniRoot) != null)
        {
            return;
        }

        Usuario root = new Usuario(dni: CargaInicial.DniRoot, nombre: "Root", apellido: "Admin",
            correo: CargaInicial.CorreoRoot, contrasena: CargaInicial.ContrasenaRoot);
        root.AsignarTipo(_repositorioTipoUsuario.ObtenerTipoPorNombre(NombreTipoAdministrador)!);
        _repositorioUsuario.AgregarUsuario(root);
    }
}
