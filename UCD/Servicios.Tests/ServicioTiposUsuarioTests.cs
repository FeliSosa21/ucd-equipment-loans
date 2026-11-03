using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioTiposUsuarioTests
{
    private const string DniAdministrador = "11111111";

    private Memoria _memoria = null!;
    private RepositorioUsuario _repositorioUsuarios = null!;
    private RepositorioTipoUsuario _repositorioTipos = null!;
    private ServicioTiposUsuario _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _memoria = new Memoria();
        _repositorioUsuarios = new RepositorioUsuario(_memoria);
        _repositorioTipos = TiposDePrueba.Cargados(_memoria);
        Usuario administrador = new Usuario(dni: DniAdministrador, nombre: "Root", apellido: "Admin",
            correo: "root@ucd.edu.uy", contrasena: "Root2026");
        administrador.AsignarTipo(_repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Administrador().Nombre)!);
        _repositorioUsuarios.AgregarUsuario(administrador);
        _servicio = new ServicioTiposUsuario(_repositorioTipos, _repositorioUsuarios);
    }

    [TestMethod]
    public void ListarTipos_SeisTiposCargados_DevuelveSusNombres()
    {
        List<TipoUsuarioDto> tipos = _servicio.ListarTipos(DniAdministrador);

        CollectionAssert.AreEqual(TipoUsuario.Todos().Select(tipo => tipo.Nombre).ToList(),
            tipos.Select(tipo => tipo.Nombre).ToList());
    }
}
