using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioSesionTests
{
    private const string DniValido = "12345672";
    private const string CorreoValido = "felipe@hotmail.com";
    private const string ContrasenaValida = "VivaLaOrt2026";

    [TestMethod]
    public void IniciarSesion_CredencialesCorrectas_DevuelveLaSesion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        Assert.AreEqual(CorreoValido, sesion.Correo);
        Assert.AreEqual("Felipe", sesion.Nombre);
        Assert.AreEqual("12345672", sesion.Dni);
    }

    [TestMethod]
    public void IniciarSesion_ContrasenaIncorrecta_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        Assert.ThrowsException<ServicioException>(() => servicio.IniciarSesion(CorreoValido, "OtraClave2026"));
    }

    [TestMethod]
    public void IniciarSesion_CorreoInexistente_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        Assert.ThrowsException<ServicioException>(() => servicio.IniciarSesion("otro@hotmail.com", ContrasenaValida));
    }

    [TestMethod]
    public void IniciarSesion_UsuarioDesactivado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        repositorio.ObtenerUsuarioPorCorreo(CorreoValido)!.Desactivar();
        ServicioSesion servicio = new ServicioSesion(repositorio);

        Assert.ThrowsException<ServicioException>(() => servicio.IniciarSesion(CorreoValido, ContrasenaValida));
    }

    [TestMethod]
    public void IniciarSesion_UsuarioRecienRegistrado_DevuelveTipoYCambioDeContrasena()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        Assert.AreEqual("Sin permisos", sesion.Tipo);
        Assert.IsFalse(sesion.DebeCambiarContrasena);
    }

    [TestMethod]
    public void IniciarSesion_UsuarioRecienRegistrado_EstaPendienteDeActivacion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        Assert.IsTrue(sesion.PendienteDeActivacion);
    }

    [TestMethod]
    public void IniciarSesion_Administrador_DevuelveElPermisoDeGestionarUsuarios()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        repositorio.ObtenerUsuarioPorCorreo(CorreoValido)!.AsignarTipo(TipoUsuario.Administrador);
        ServicioSesion servicio = new ServicioSesion(repositorio);

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, ContrasenaValida);

        CollectionAssert.Contains(sesion.Permisos, nameof(Permiso.GestionarUsuarios));
    }

    [TestMethod]
    public void CambiarContrasena_DatosCorrectos_PermiteIngresarConLaNueva()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        servicio.CambiarContrasena(CorreoValido, ContrasenaValida, "NuevaClave2026");

        UsuarioSesionDto sesion = servicio.IniciarSesion(CorreoValido, "NuevaClave2026");
        Assert.IsFalse(sesion.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CambiarContrasena_ActualIncorrecta_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(CorreoValido, "OtraClave2026", "NuevaClave2026"));
    }

    [TestMethod]
    public void CambiarContrasena_NuevaInvalida_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(CorreoValido, ContrasenaValida, "corta1A"));
    }

    [TestMethod]
    public void CambiarContrasena_CorreoInexistente_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        ServicioSesion servicio = new ServicioSesion(repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena("ana@hotmail.com", ContrasenaValida, "NuevaClave2026"));
    }

    [TestMethod]
    public void RenovarSesion_UsuarioActivo_DevuelveLosDatosActuales()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        repositorio.ObtenerUsuarioPorDni(DniValido)!.AsignarTipo(TipoUsuario.Docente);
        ServicioSesion servicio = new ServicioSesion(repositorio);

        UsuarioSesionDto sesion = servicio.RenovarSesion(DniValido);

        Assert.AreEqual(TipoUsuario.Docente.Nombre, sesion.Tipo);
        Assert.IsFalse(sesion.PendienteDeActivacion);
    }

    [TestMethod]
    public void RenovarSesion_UsuarioDesactivado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        new ServicioUsuario(repositorio).Registrar(CrearDto());
        repositorio.ObtenerUsuarioPorDni(DniValido)!.Desactivar();
        ServicioSesion servicio = new ServicioSesion(repositorio);

        Assert.ThrowsException<ServicioException>(() => servicio.RenovarSesion(DniValido));
    }

    [TestMethod]
    public void RenovarSesion_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioSesion servicio = new ServicioSesion(new RepositorioUsuario(new Memoria()));

        Assert.ThrowsException<ServicioException>(() => servicio.RenovarSesion(DniValido));
    }

    private static UsuarioDto CrearDto()
    {
        return new UsuarioDto
        {
            Dni = DniValido, Nombre = "Felipe", Apellido = "Sosa", Correo = CorreoValido, Contrasena = ContrasenaValida
        };
    }
}
