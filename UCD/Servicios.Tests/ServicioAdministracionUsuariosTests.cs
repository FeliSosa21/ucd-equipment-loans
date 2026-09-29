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
