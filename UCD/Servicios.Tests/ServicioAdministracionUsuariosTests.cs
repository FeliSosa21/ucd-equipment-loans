using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioAdministracionUsuariosTests
{
    private const string DniValido = "12345672";
    private const string CorreoNuevo = "ana@hotmail.com";

    [TestMethod]
    public void ListarUsuarios_ConUnRegistrado_LoDevuelveConSuTipoYEstado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        List<UsuarioResumenDto> usuarios = servicio.ListarUsuarios();

        Assert.AreEqual(1, usuarios.Count);
        Assert.AreEqual(DniValido, usuarios[0].Dni);
        Assert.AreEqual(TipoUsuario.SinPermisos.Nombre, usuarios[0].Tipo);
        Assert.IsTrue(usuarios[0].Activo);
    }

    [TestMethod]
    public void AsignarTipo_TipoExistente_LoAsigna()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);

        servicio.AsignarTipo(DniValido, TipoUsuario.Docente.Nombre);

        Assert.AreEqual(TipoUsuario.Docente, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo);
    }

    [TestMethod]
    public void AsignarTipo_TipoInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.AsignarTipo(DniValido, "Rector"));
    }

    [TestMethod]
    public void AsignarTipo_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo("45678905", TipoUsuario.Docente.Nombre));
    }

    [TestMethod]
    public void Desactivar_UsuarioActivo_QuedaDesactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);

        servicio.Desactivar(DniValido);

        Assert.AreEqual(EstadoUsuario.Desactivado, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Reactivar_UsuarioDesactivado_QuedaActivo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);
        servicio.Desactivar(DniValido);

        servicio.Reactivar(DniValido);

        Assert.AreEqual(EstadoUsuario.Activo, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void CrearUsuario_DatosValidos_QuedaConElTipoYLaContrasenaPredeterminada()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);

        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante.Nombre);

        Usuario? creado = repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo);
        Assert.IsNotNull(creado);
        Assert.AreEqual(TipoUsuario.Estudiante, creado.Tipo);
        Assert.IsTrue(creado.ContrasenaEsCorrecta(ServicioAdministracionUsuarios.ContrasenaPredeterminada));
        Assert.IsTrue(creado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CrearUsuario_DniDuplicado_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);
        UsuarioDto dto = CrearDtoNuevo();
        dto.Dni = DniValido;

        Assert.ThrowsException<ServicioException>(() => servicio.CrearUsuario(dto, TipoUsuario.Estudiante.Nombre));
    }

    [TestMethod]
    public void CrearUsuario_TipoInexistente_NoLoGuarda()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);

        Assert.ThrowsException<ServicioException>(() => servicio.CrearUsuario(CrearDtoNuevo(), "Rector"));
        Assert.IsNull(repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo));
    }

    [TestMethod]
    public void ObtenerResumen_UsuarioExistente_DevuelveSusDatos()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        UsuarioResumenDto resumen = servicio.ObtenerResumen(DniValido);

        Assert.AreEqual("Felipe", resumen.Nombre);
    }

    [TestMethod]
    public void ModificarDatos_DatosValidos_LosActualiza()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);

        servicio.ModificarDatos(DniValido, "Pedro", "Suarez", "pedro@hotmail.com");

        Usuario usuario = repositorio.ObtenerUsuarioPorDni(DniValido)!;
        Assert.AreEqual("Pedro", usuario.Nombre);
        Assert.AreEqual("pedro@hotmail.com", usuario.Correo);
    }

    [TestMethod]
    public void ModificarDatos_MismoCorreoConOtrasMayusculas_LoPermite()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);

        servicio.ModificarDatos(DniValido, "Felipe", "Sosa", "FELIPE@hotmail.com");

        Assert.AreEqual("FELIPE@hotmail.com", repositorio.ObtenerUsuarioPorDni(DniValido)!.Correo);
    }

    [TestMethod]
    public void ModificarDatos_CorreoDeOtroUsuario_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante.Nombre);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(DniValido, "Felipe", "Sosa", CorreoNuevo));
    }

    [TestMethod]
    public void ModificarDatos_NombreInvalido_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(DniValido, "F", "Sosa", "felipe@hotmail.com"));
    }

    [TestMethod]
    public void Desactivar_UnicoAdministradorActivo_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador.Nombre);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniValido));
        Assert.AreEqual(EstadoUsuario.Activo, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Desactivar_AdministradorConOtroAdministradorActivo_QuedaDesactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador.Nombre);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador.Nombre);

        servicio.Desactivar(DniValido);

        Assert.AreEqual(EstadoUsuario.Desactivado, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void AsignarTipo_UnicoAdministradorActivoAOtroTipo_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador.Nombre);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente.Nombre));
        Assert.AreEqual(TipoUsuario.Administrador, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo);
    }

    [TestMethod]
    public void AsignarTipo_UnicoAdministradorActivoAAdministrador_LoMantiene()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador.Nombre);

        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador.Nombre);

        Assert.AreEqual(TipoUsuario.Administrador, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo);
    }

    [TestMethod]
    public void Desactivar_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar("45678905"));
    }

    [TestMethod]
    public void Reactivar_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar("45678905"));
    }

    [TestMethod]
    public void ObtenerResumen_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.ObtenerResumen("45678905"));
    }

    [TestMethod]
    public void ModificarDatos_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConUnUsuario(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos("45678905", "Pedro", "Suarez", "pedro@hotmail.com"));
    }

    private static UsuarioDto CrearDtoNuevo()
    {
        return new UsuarioDto
        {
            Dni = "45678905", Nombre = "Ana", Apellido = "Perez", Correo = CorreoNuevo
        };
    }

    private static ServicioAdministracionUsuarios CrearServicioConUnUsuario(out RepositorioUsuario repositorio)
    {
        Memoria memoria = new Memoria();
        repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicioUsuario = new ServicioUsuario(repositorio);
        servicioUsuario.Registrar(new UsuarioDto
        {
            Dni = DniValido, Nombre = "Felipe", Apellido = "Sosa", Correo = "felipe@hotmail.com",
            Contrasena = "VivaLaOrt2026"
        });
        return new ServicioAdministracionUsuarios(repositorio, servicioUsuario);
    }
}
