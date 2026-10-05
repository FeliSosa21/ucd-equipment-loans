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
    private const string DniResponsable = "11111111";
    private const string DniNuevo = "45678905";

    [TestMethod]
    public void ListarUsuarios_ConUnRegistradoYElAdministrador_LosDevuelveConSuTipoYEstado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<UsuarioResumenDto> usuarios = servicio.ListarUsuarios();

        UsuarioResumenDto registrado = usuarios.Single(usuario => usuario.Dni == DniValido);
        Assert.AreEqual(2, usuarios.Count);
        Assert.AreEqual(TipoUsuario.SinPermisos.Nombre, registrado.Tipo);
        Assert.IsTrue(registrado.Activo);
    }

    [TestMethod]
    public void NombresDeTipos_DevuelveLosSeisTipos()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<string> nombres = servicio.NombresDeTipos();

        CollectionAssert.AreEqual(new List<string>
        {
            "Sin permisos", "Estudiante", "Docente", "Funcionario del centro de equipamiento", "Supervisor",
            "Administrador"
        }, nombres);
    }

    [TestMethod]
    public void AsignarTipo_TipoExistente_LoAsigna()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        servicio.AsignarTipo(DniValido, TipoUsuario.Docente.Nombre, DniResponsable);

        Assert.AreEqual(TipoUsuario.Docente, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo);
    }

    [TestMethod]
    public void AsignarTipo_TipoInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.AsignarTipo(DniValido, "Rector", DniResponsable));
    }

    [TestMethod]
    public void AsignarTipo_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo("45678905", TipoUsuario.Docente.Nombre, DniResponsable));
    }

    [TestMethod]
    public void Desactivar_UsuarioActivo_QuedaDesactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        servicio.Desactivar(DniValido, DniResponsable);

        Assert.AreEqual(EstadoUsuario.Desactivado, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Reactivar_UsuarioDesactivado_QuedaActivo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.Desactivar(DniValido, DniResponsable);

        servicio.Reactivar(DniValido, DniResponsable);

        Assert.AreEqual(EstadoUsuario.Activo, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void CrearUsuario_DatosValidos_QuedaConElTipoYLaContrasenaPredeterminada()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante.Nombre, DniResponsable);

        Usuario? creado = repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo);
        Assert.IsNotNull(creado);
        Assert.AreEqual(TipoUsuario.Estudiante, creado.Tipo);
        Assert.IsTrue(creado.ContrasenaEsCorrecta(ServicioAdministracionUsuarios.ContrasenaPredeterminada));
        Assert.IsTrue(creado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CrearUsuario_DniDuplicado_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        UsuarioDto dto = CrearDtoNuevo();
        dto.Dni = DniValido;

        Assert.ThrowsException<ServicioException>(() => servicio.CrearUsuario(dto, TipoUsuario.Estudiante.Nombre,
            DniResponsable));
    }

    [TestMethod]
    public void CrearUsuario_TipoInexistente_NoLoGuarda()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CrearUsuario(CrearDtoNuevo(), "Rector", DniResponsable));
        Assert.IsNull(repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo));
    }

    [TestMethod]
    public void ObtenerResumen_UsuarioExistente_DevuelveSusDatos()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        UsuarioResumenDto resumen = servicio.ObtenerResumen(DniValido);

        Assert.AreEqual("Felipe", resumen.Nombre);
    }

    [TestMethod]
    public void ModificarDatos_DatosValidos_LosActualiza()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        servicio.ModificarDatos(
            CrearDatos(dni: DniValido, nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"), DniValido);

        Usuario usuario = repositorio.ObtenerUsuarioPorDni(DniValido)!;
        Assert.AreEqual("Pedro", usuario.Nombre);
        Assert.AreEqual("pedro@hotmail.com", usuario.Correo);
    }

    [TestMethod]
    public void ModificarDatos_MismoCorreoConOtrasMayusculas_LoPermite()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        servicio.ModificarDatos(
            CrearDatos(dni: DniValido, nombre: "Felipe", apellido: "Sosa", correo: "FELIPE@hotmail.com"), DniValido);

        Assert.AreEqual("FELIPE@hotmail.com", repositorio.ObtenerUsuarioPorDni(DniValido)!.Correo);
    }

    [TestMethod]
    public void ModificarDatos_CorreoDeOtroUsuario_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante.Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: DniValido, nombre: "Felipe", apellido: "Sosa", correo: CorreoNuevo), DniValido));
    }

    [TestMethod]
    public void ModificarDatos_NombreInvalido_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: DniValido, nombre: "F", apellido: "Sosa", correo: "felipe@hotmail.com"), DniValido));
    }

    [TestMethod]
    public void Desactivar_AdministradorConOtroAdministradorActivo_QuedaDesactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador.Nombre, DniResponsable);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador.Nombre, DniResponsable);

        servicio.Desactivar(DniValido, DniResponsable);

        Assert.AreEqual(EstadoUsuario.Desactivado, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void AsignarTipo_AdministradorAAdministrador_LoMantiene()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador.Nombre, DniResponsable);

        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador.Nombre, DniResponsable);

        Assert.AreEqual(TipoUsuario.Administrador, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo);
    }

    [TestMethod]
    public void Desactivar_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar("45678905", DniResponsable));
    }

    [TestMethod]
    public void Reactivar_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar("45678905", DniResponsable));
    }

    [TestMethod]
    public void ObtenerResumen_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.ObtenerResumen("45678905"));
    }

    [TestMethod]
    public void ModificarDatos_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: "45678905", nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"),
                "45678905"));
    }

    [TestMethod]
    public void Desactivar_ElMismoResponsable_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniValido, DniValido));
    }

    [TestMethod]
    public void AsignarTipo_ElMismoResponsable_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente.Nombre, DniValido));
    }

    [TestMethod]
    public void ModificarDatos_AdministradorModificaAOtro_LosActualiza()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador.Nombre, DniResponsable);

        servicio.ModificarDatos(CrearDatos(dni: DniValido, nombre: "Pedro", apellido: "Suarez",
            correo: "pedro@hotmail.com"), "45678905");

        Assert.AreEqual("Pedro", repositorio.ObtenerUsuarioPorDni(DniValido)!.Nombre);
    }

    [TestMethod]
    public void ModificarDatos_NoAdministradorModificaAOtro_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante.Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.ModificarDatos(CrearDatos(dni: DniValido,
            nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"), "45678905"));
        Assert.AreEqual("Felipe", repositorio.ObtenerUsuarioPorDni(DniValido)!.Nombre);
    }

    [TestMethod]
    public void CrearUsuario_DtoNulo_LanzaArgumentNullException()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ArgumentNullException>(() =>
            servicio.CrearUsuario(null!, TipoUsuario.Estudiante.Nombre, DniResponsable));
    }

    [TestMethod]
    public void ModificarDatos_DatosNulos_LanzaArgumentNullException()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ModificarDatos(null!, DniValido));
    }

    [TestMethod]
    public void ModificarDatos_NombreInvalido_ConservaLaExcepcionDelDominio()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: DniValido, nombre: "F", apellido: "Sosa", correo: "felipe@hotmail.com"), DniValido));

        Assert.IsInstanceOfType(excepcion.InnerException, typeof(DominioException));
    }

    [TestMethod]
    public void CrearUsuario_ResponsableNoAdministrador_NoLoGuarda()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante.Nombre, DniValido));
        Assert.IsNull(repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo));
    }

    [TestMethod]
    public void AsignarTipo_ResponsableNoAdministrador_NoLoCambia()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante.Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente.Nombre, DniNuevo));
        Assert.AreEqual(TipoUsuario.SinPermisos, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo);
    }

    [TestMethod]
    public void AsignarTipo_ResponsableDesactivado_NoLoCambia()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador.Nombre, DniResponsable);
        servicio.Desactivar(DniResponsable, DniNuevo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente.Nombre, DniResponsable));
        Assert.AreEqual(TipoUsuario.SinPermisos, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo);
    }

    [TestMethod]
    public void Desactivar_ResponsableDesactivado_NoLoDesactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador.Nombre, DniResponsable);
        servicio.Desactivar(DniResponsable, DniNuevo);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniValido, DniResponsable));
        Assert.AreEqual(EstadoUsuario.Activo, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Reactivar_ResponsableNoAdministrador_NoLoReactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.Desactivar(DniValido, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(DniValido, DniNuevo));
        Assert.AreEqual(EstadoUsuario.Desactivado, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Desactivar_UnicoAdministradorActivoASiMismo_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniResponsable, DniResponsable));
        Assert.AreEqual(EstadoUsuario.Activo, repositorio.ObtenerUsuarioPorDni(DniResponsable)!.Estado);
    }

    private static UsuarioResumenDto CrearDatos(string dni, string nombre, string apellido, string correo)
    {
        return new UsuarioResumenDto { Dni = dni, Nombre = nombre, Apellido = apellido, Correo = correo };
    }

    private static UsuarioDto CrearDtoNuevo()
    {
        return new UsuarioDto
        {
            Dni = "45678905", Nombre = "Ana", Apellido = "Perez", Correo = CorreoNuevo
        };
    }

    private static ServicioAdministracionUsuarios CrearServicioConAdministrador(out RepositorioUsuario repositorio)
    {
        Memoria memoria = new Memoria();
        repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicioUsuario = new ServicioUsuario(repositorio);
        servicioUsuario.Registrar(new UsuarioDto
        {
            Dni = DniValido, Nombre = "Felipe", Apellido = "Sosa", Correo = "felipe@hotmail.com",
            Contrasena = "VivaLaOrt2026"
        });
        Usuario administrador = new Usuario(dni: DniResponsable, nombre: "Root", apellido: "Admin",
            correo: "root@ucd.edu.uy", contrasena: "Root2026");
        administrador.AsignarTipo(TipoUsuario.Administrador);
        repositorio.AgregarUsuario(administrador);
        return new ServicioAdministracionUsuarios(repositorio, servicioUsuario);
    }
}
