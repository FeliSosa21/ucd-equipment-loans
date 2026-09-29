using Data;
using Data.Repositorios;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioSesionTests
{
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

    private static UsuarioDto CrearDto()
    {
        return new UsuarioDto
        {
            Dni = "12345672", Nombre = "Felipe", Apellido = "Sosa", Correo = CorreoValido, Contrasena = ContrasenaValida
        };
    }
}
