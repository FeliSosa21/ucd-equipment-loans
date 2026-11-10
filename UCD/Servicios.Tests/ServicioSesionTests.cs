using Data;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

[TestClass]
public class ServicioSesionTests
{
    private const string DniValido = "12345672";
    private const string CorreoValido = "felipe@hotmail.com";
    private const string ContrasenaValida = "VivaLaOrt2026";
    private const string ContrasenaNueva = "NuevaClave2026";

    private DbContextOptions<SqlContext> _opciones = null!;
    private SqlContext _contexto = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using SqlContext preparacion = new SqlContext(_opciones);
        TiposDePrueba.GuardarEn(preparacion);
        _contexto = new SqlContext(_opciones);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void IniciarSesion_CredencialesCorrectas_DevuelveLaSesion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        Assert.AreEqual(CorreoValido, sesion.Correo);
        Assert.AreEqual("Felipe", sesion.Nombre);
        Assert.AreEqual(DniValido, sesion.Dni);
    }

    [TestMethod]
    public void IniciarSesion_ContrasenaIncorrecta_LanzaExcepcion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.IniciarSesion(CorreoValido, "OtraClave2026"));
    }

    [TestMethod]
    public void IniciarSesion_CorreoInexistente_LanzaExcepcion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.IniciarSesion("otro@hotmail.com", ContrasenaValida));
    }

    [TestMethod]
    public void IniciarSesion_UsuarioDesactivado_LanzaExcepcion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuarioSql repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.Desactivar();
        _contexto.SaveChanges();

        Assert.ThrowsException<ServicioException>(() => servicio.IniciarSesion(CorreoValido, ContrasenaValida));
    }

    [TestMethod]
    public void IniciarSesion_UsuarioRecienRegistrado_TieneTipoSinPermisos()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        Assert.AreEqual("Sin permisos", sesion.Tipo);
    }

    [TestMethod]
    public void IniciarSesion_UsuarioRecienRegistrado_NoDebeCambiarContrasena()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        Assert.IsFalse(sesion.DebeCambiarContrasena);
    }

    [TestMethod]
    public void IniciarSesion_UsuarioRecienRegistrado_EstaPendienteDeActivacion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        Assert.IsTrue(sesion.PendienteDeActivacion);
    }

    [TestMethod]
    public void IniciarSesion_Administrador_DevuelveElPermisoDeGestionarUsuarios()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuarioSql repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.AsignarTipo(ObtenerTipo(TipoUsuario.Administrador().Nombre));
        _contexto.SaveChanges();

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        CollectionAssert.Contains(sesion.Permisos, nameof(Permiso.GestionarUsuarios));
    }

    [TestMethod]
    public void CambiarContrasena_DatosCorrectos_PermiteIngresarConLaNueva()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        servicio.CambiarContrasena(DniValido, ContrasenaValida, ContrasenaNueva);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaNueva);
        Assert.IsFalse(sesion.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CambiarContrasena_CambioObligatorio_PersisteLaNuevaYLiberaLaObligacion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);
        ExigirCambioDeContrasena();

        servicio.CambiarContrasena(DniValido, ContrasenaValida, ContrasenaNueva);

        Usuario guardado = LeerUsuario();
        Assert.IsTrue(guardado.ContrasenaEsCorrecta(ContrasenaNueva));
        Assert.IsFalse(guardado.ContrasenaEsCorrecta(ContrasenaValida));
        Assert.IsFalse(guardado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CambiarContrasena_CorreoModificado_IdentificaAlUsuarioPorDni()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuarioSql repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.ModificarDatos("Felipe", "Sosa", "nuevo@hotmail.com");
        _contexto.SaveChanges();

        servicio.CambiarContrasena(DniValido, ContrasenaValida, ContrasenaNueva);

        Assert.IsTrue(LeerUsuario().ContrasenaEsCorrecta(ContrasenaNueva));
    }

    [TestMethod]
    public void CambiarContrasena_ActualIncorrecta_LanzaExcepcionYConservaLosDatosGuardados()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);
        ExigirCambioDeContrasena();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(DniValido, "OtraClave2026", ContrasenaNueva));

        Usuario guardado = LeerUsuario();
        Assert.IsTrue(guardado.ContrasenaEsCorrecta(ContrasenaValida));
        Assert.IsFalse(guardado.ContrasenaEsCorrecta(ContrasenaNueva));
        Assert.IsTrue(guardado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CambiarContrasena_ActualIncorrecta_ElErrorIndicaElCampoContrasenaActual()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(DniValido, "OtraClave2026", ContrasenaNueva));

        Assert.AreEqual("ContrasenaActual", excepcion.Campo);
    }

    [TestMethod]
    public void CambiarContrasena_NuevaInvalida_ConservaLaExcepcionDelDominioYLosDatosGuardados()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);
        ExigirCambioDeContrasena();

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(DniValido, ContrasenaValida, "corta1A"));

        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));
        Usuario guardado = LeerUsuario();
        Assert.IsTrue(guardado.ContrasenaEsCorrecta(ContrasenaValida));
        Assert.IsTrue(guardado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CambiarContrasena_DniInexistente_LanzaExcepcion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena("45678905", ContrasenaValida, ContrasenaNueva));
    }

    [TestMethod]
    public void CambiarContrasena_UsuarioDesactivado_LanzaExcepcionYNoLaCambia()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuarioSql repositorio);
        Usuario usuario = repositorio.ObtenerUsuarioPorDni(DniValido)!;
        usuario.Desactivar();
        _contexto.SaveChanges();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(DniValido, ContrasenaValida, ContrasenaNueva));

        Assert.IsTrue(LeerUsuario().ContrasenaEsCorrecta(ContrasenaValida));
    }

    [TestMethod]
    public void RenovarSesion_UsuarioActivo_DevuelveLosDatosActuales()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuarioSql repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.AsignarTipo(ObtenerTipo(TipoUsuario.Docente().Nombre));
        _contexto.SaveChanges();

        UsuarioSesionDto sesion = servicio.RenovarSesion(DniValido);

        Assert.AreEqual(TipoUsuario.Docente().Nombre, sesion.Tipo);
        Assert.IsFalse(sesion.PendienteDeActivacion);
    }

    [TestMethod]
    public void RenovarSesion_UsuarioDesactivado_LanzaExcepcion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuarioSql repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.Desactivar();
        _contexto.SaveChanges();

        Assert.ThrowsException<ServicioException>(() => servicio.RenovarSesion(DniValido));
    }

    [TestMethod]
    public void RenovarSesion_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioSesion servicio = new ServicioSesion(new RepositorioUsuarioSql(_contexto));

        Assert.ThrowsException<ServicioException>(() => servicio.RenovarSesion(DniValido));
    }

    private ServicioSesion CrearServicioConUsuarioRegistrado(out RepositorioUsuarioSql repositorio)
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        Usuario usuario = new Usuario(dni: DniValido, nombre: "Felipe", apellido: "Sosa",
            correo: CorreoValido, contrasena: ContrasenaValida);
        usuario.AsignarTipo(new RepositorioTipoUsuarioSql(preparacion)
            .ObtenerTipoPorNombre(TipoUsuario.SinPermisos().Nombre)!);
        new RepositorioUsuarioSql(preparacion).AgregarUsuario(usuario);
        repositorio = new RepositorioUsuarioSql(_contexto);
        return new ServicioSesion(repositorio);
    }

    private void ExigirCambioDeContrasena()
    {
        using SqlContext preparacion = new SqlContext(_opciones);
        RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
        Usuario usuario = usuarios.ObtenerUsuarioPorDni(DniValido)!;
        usuario.ExigirCambioDeContrasena();
        usuarios.ActualizarUsuario(usuario);
    }

    private Usuario LeerUsuario()
    {
        using SqlContext lectura = new SqlContext(_opciones);
        return new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorDni(DniValido)!;
    }

    private TipoUsuario ObtenerTipo(string nombre)
    {
        return new RepositorioTipoUsuarioSql(_contexto).ObtenerTipoPorNombre(nombre)!;
    }
}
