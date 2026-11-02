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
    private const string ContrasenaNueva = "NuevaClave2026";

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
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuario repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.Desactivar();

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
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuario repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.AsignarTipo(TipoUsuario.Administrador);

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
    public void CambiarContrasena_CorreoModificado_IdentificaAlUsuarioPorDni()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuario repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.ModificarDatos("Felipe", "Sosa", "nuevo@hotmail.com");

        servicio.CambiarContrasena(DniValido, ContrasenaValida, ContrasenaNueva);

        Assert.IsTrue(repositorio.ObtenerUsuarioPorDni(DniValido)!.ContrasenaEsCorrecta(ContrasenaNueva));
    }

    [TestMethod]
    public void CambiarContrasena_ActualIncorrecta_LanzaExcepcion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(DniValido, "OtraClave2026", ContrasenaNueva));
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
    public void CambiarContrasena_NuevaInvalida_LanzaExcepcionConLaDelDominio()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out _);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(DniValido, ContrasenaValida, "corta1A"));

        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));
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
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuario repositorio);
        Usuario usuario = repositorio.ObtenerUsuarioPorDni(DniValido)!;
        usuario.Desactivar();

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CambiarContrasena(DniValido, ContrasenaValida, ContrasenaNueva));
        Assert.IsTrue(usuario.ContrasenaEsCorrecta(ContrasenaValida));
    }

    [TestMethod]
    public void RenovarSesion_UsuarioActivo_DevuelveLosDatosActuales()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuario repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.AsignarTipo(TipoUsuario.Docente);

        UsuarioSesionDto sesion = servicio.RenovarSesion(DniValido);

        Assert.AreEqual(TipoUsuario.Docente.Nombre, sesion.Tipo);
        Assert.IsFalse(sesion.PendienteDeActivacion);
    }

    [TestMethod]
    public void RenovarSesion_UsuarioDesactivado_LanzaExcepcion()
    {
        ServicioSesion servicio = CrearServicioConUsuarioRegistrado(out RepositorioUsuario repositorio);
        repositorio.ObtenerUsuarioPorDni(DniValido)!.Desactivar();

        Assert.ThrowsException<ServicioException>(() => servicio.RenovarSesion(DniValido));
    }

    [TestMethod]
    public void RenovarSesion_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioSesion servicio = new ServicioSesion(new RepositorioUsuario(new Memoria()));

        Assert.ThrowsException<ServicioException>(() => servicio.RenovarSesion(DniValido));
    }

    private static ServicioSesion CrearServicioConUsuarioRegistrado(out RepositorioUsuario repositorio)
    {
        repositorio = new RepositorioUsuario(new Memoria());
        new ServicioUsuario(repositorio).Registrar(new UsuarioDto
        {
            Dni = DniValido,
            Nombre = "Felipe",
            Apellido = "Sosa",
            Correo = CorreoValido,
            Contrasena = ContrasenaValida
        });

        return new ServicioSesion(repositorio);
    }
}
