using Data;
using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

[TestClass]
public class CargaInicialSqlTests
{
    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    }

    [TestMethod]
    public void CargarRoot_AdministradorPersistido_GuardaRootSinCambioObligatorio()
    {
        GuardarAdministrador();

        CargarRootConOtroContexto();

        using SqlContext lectura = new SqlContext(_opciones);
        Usuario? root = new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorDni(CargaInicialSql.DniRoot);
        Assert.IsNotNull(root);
        Assert.AreEqual(CargaInicialSql.CorreoRoot, root.Correo);
        Assert.AreEqual("Administrador", root.Tipo.Nombre);
        Assert.IsFalse(root.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CargarRoot_AdministradorConIdGenerado_UsaElTipoPersistido()
    {
        int idAdministrador = GuardarAdministrador();

        CargarRootConOtroContexto();

        using SqlContext lectura = new SqlContext(_opciones);
        Usuario? root = new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorDni(CargaInicialSql.DniRoot);
        Assert.IsNotNull(root);
        Assert.AreEqual(idAdministrador, root.Tipo.Id);
        Assert.AreEqual(2, lectura.TiposDeUsuario.Count());
    }

    [TestMethod]
    public void CargarRoot_DosArranques_NoDuplicaElRoot()
    {
        GuardarAdministrador();

        CargarRootConOtroContexto();
        CargarRootConOtroContexto();

        using SqlContext lectura = new SqlContext(_opciones);
        Assert.AreEqual(1, lectura.Usuarios.Count());
        Assert.AreEqual(CargaInicialSql.DniRoot, lectura.Usuarios.Single().Dni);
    }

    [TestMethod]
    public void CargarRoot_CorreoModificado_ConservaElCorreoYNoDuplicaPorDni()
    {
        GuardarAdministrador();
        CargarRootConOtroContexto();
        string correoModificado = "root.actualizado@ucd.edu.uy";
        using (SqlContext modificacion = new SqlContext(_opciones))
        {
            RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(modificacion);
            Usuario root = usuarios.ObtenerUsuarioPorDni(CargaInicialSql.DniRoot)!;
            root.ModificarDatos(root.Nombre, root.Apellido, correoModificado);
            usuarios.ActualizarUsuario(root);
        }

        CargarRootConOtroContexto();

        using SqlContext lectura = new SqlContext(_opciones);
        Usuario? guardado = new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorDni(CargaInicialSql.DniRoot);
        Assert.IsNotNull(guardado);
        Assert.AreEqual(correoModificado, guardado.Correo);
        Assert.AreEqual(1, lectura.Usuarios.Count());
    }

    [TestMethod]
    public void CargarRoot_SinAdministrador_InformaErrorDeInstalacionYNoCreaDatos()
    {
        using (SqlContext preparacion = new SqlContext(_opciones))
        {
            preparacion.Database.EnsureCreated();
        }

        InvalidOperationException error = Assert.ThrowsException<InvalidOperationException>(CargarRootConOtroContexto);

        Assert.AreEqual("Falta el tipo de usuario Administrador. Ejecute el script de datos de la carpeta Respaldos " +
            "antes de iniciar la aplicación (ver sección Instalación de la documentación).", error.Message);
        using SqlContext lectura = new SqlContext(_opciones);
        Assert.AreEqual(0, lectura.Usuarios.Count());
        Assert.AreEqual(0, lectura.TiposDeUsuario.Count());
    }

    private int GuardarAdministrador()
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        RepositorioTipoUsuarioSql tipos = new RepositorioTipoUsuarioSql(preparacion);
        tipos.AgregarTipo(TipoUsuario.SinPermisos());
        TipoUsuario administrador = TipoUsuario.Administrador();
        tipos.AgregarTipo(administrador);
        return administrador.Id;
    }

    private void CargarRootConOtroContexto()
    {
        using SqlContext operacion = new SqlContext(_opciones);
        CargaInicialSql carga = new CargaInicialSql(new RepositorioUsuarioSql(operacion),
            new RepositorioTipoUsuarioSql(operacion));
        carga.CargarRoot();
    }
}
