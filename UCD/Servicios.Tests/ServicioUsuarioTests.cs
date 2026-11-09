using Data;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

[TestClass]
public class ServicioUsuarioTests
{
    private static readonly DateTime Ahora = new(2026, 11, 8, 9, 30, 0);
    private DbContextOptions<SqlContext> _opciones = null!;
    private SqlContext _contexto = null!;
    private ServicioUsuario _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _contexto = new SqlContext(_opciones);
        _contexto.Database.EnsureCreated();
        RegistradorDeAuditoria auditoria = new RegistradorDeAuditoria(
            new RepositorioAuditoriaSql(_contexto), new RelojFijo(Ahora));
        _servicio = new ServicioUsuario(new RepositorioUsuarioSql(_contexto),
            new RepositorioTipoUsuarioSql(_contexto), auditoria);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void Registrar_DatosValidos_SeGuarda()
    {
        UsuarioDto dto = CrearDto();

        _servicio.Registrar(dto);

        Assert.IsNotNull(LeerUsuarioPorDni(dto.Dni));
    }

    [TestMethod]
    public void Registrar_DatosValidos_LeAsignaSinPermisosDelRepositorio()
    {
        UsuarioDto dto = CrearDto();

        _servicio.Registrar(dto);

        using SqlContext lectura = new SqlContext(_opciones);
        TipoUsuario sinPermisos = new RepositorioTipoUsuarioSql(lectura)
            .ObtenerTipoPorNombre(TipoUsuario.SinPermisos().Nombre)!;
        Assert.AreEqual(sinPermisos.Id, LeerUsuarioPorDni(dto.Dni)!.Tipo.Id);
    }

    [TestMethod]
    public void Registrar_DniDuplicado_LanzaExcepcion()
    {
        UsuarioDto primero = CrearDto();
        UsuarioDto repetido = CrearDto(correo: "otro@hotmail.com");
        _servicio.Registrar(primero);

        Assert.ThrowsException<ServicioException>(() => _servicio.Registrar(repetido));
        Assert.IsNull(LeerUsuarioPorCorreo(repetido.Correo));
    }

    [TestMethod]
    public void Registrar_DniDuplicadoSinCeroAdelante_LanzaExcepcion()
    {
        UsuarioDto primero = CrearDto(dni: "01234561");
        UsuarioDto repetido = CrearDto(dni: "1234561", correo: "otro@hotmail.com");
        _servicio.Registrar(primero);

        Assert.ThrowsException<ServicioException>(() => _servicio.Registrar(repetido));
        Assert.IsNull(LeerUsuarioPorCorreo(repetido.Correo));
    }

    [TestMethod]
    public void Registrar_CorreoDuplicadoConOtrasMayusculas_LanzaExcepcion()
    {
        UsuarioDto primero = CrearDto();
        UsuarioDto repetido = CrearDto(dni: "45678905", correo: "Felipe@Hotmail.COM");
        _servicio.Registrar(primero);

        Assert.ThrowsException<ServicioException>(() => _servicio.Registrar(repetido));
        Assert.IsNull(LeerUsuarioPorDni(repetido.Dni));
    }

    [TestMethod]
    public void Registrar_DniInvalido_LanzaExcepcion()
    {
        UsuarioDto dto = CrearDto(dni: "12345678");

        Assert.ThrowsException<ServicioException>(() => _servicio.Registrar(dto));
    }

    [TestMethod]
    public void Registrar_DniInvalido_ElErrorIndicaElCampoDni()
    {
        UsuarioDto dto = CrearDto(dni: "12345678");

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() => _servicio.Registrar(dto));

        Assert.AreEqual("Dni", excepcion.Campo);
    }

    [TestMethod]
    public void Registrar_DniDuplicado_ElErrorIndicaElCampoDni()
    {
        _servicio.Registrar(CrearDto());

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            _servicio.Registrar(CrearDto(correo: "otro@hotmail.com")));

        Assert.AreEqual("Dni", excepcion.Campo);
    }

    [TestMethod]
    public void Registrar_DniDeUsuarioEliminado_LanzaServicioExceptionGeneral()
    {
        UsuarioDto original = CrearDto();
        _servicio.Registrar(original);
        using (SqlContext preparacion = new SqlContext(_opciones))
        {
            RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
            usuarios.EliminarUsuario(usuarios.ObtenerUsuarioPorDni(original.Dni)!);
        }
        UsuarioDto repetido = CrearDto(correo: "otro@hotmail.com");

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() => _servicio.Registrar(repetido));

        Assert.IsNull(excepcion.Campo);
        Assert.AreEqual("Ya existe un usuario con ese DNI o correo", excepcion.Message);
        Assert.IsNull(LeerUsuarioPorCorreo(repetido.Correo));
        Assert.AreEqual(1, LeerLog().Count);
        using SqlContext lectura = new SqlContext(_opciones);
        Assert.IsTrue(lectura.Usuarios.Single().EstaEliminado);
    }

    [TestMethod]
    public void Registrar_CorreoDeUsuarioEliminadoConOtrasMayusculas_LanzaServicioExceptionGeneral()
    {
        UsuarioDto original = CrearDto();
        _servicio.Registrar(original);
        using (SqlContext preparacion = new SqlContext(_opciones))
        {
            RepositorioUsuarioSql usuarios = new RepositorioUsuarioSql(preparacion);
            usuarios.EliminarUsuario(usuarios.ObtenerUsuarioPorDni(original.Dni)!);
        }
        UsuarioDto repetido = CrearDto(dni: "45678905", correo: "Felipe@Hotmail.COM");

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() => _servicio.Registrar(repetido));

        Assert.IsNull(excepcion.Campo);
        Assert.AreEqual("Ya existe un usuario con ese DNI o correo", excepcion.Message);
        Assert.IsNull(LeerUsuarioPorDni(repetido.Dni));
        Assert.AreEqual(1, LeerLog().Count);
        using SqlContext lectura = new SqlContext(_opciones);
        Assert.IsTrue(lectura.Usuarios.Single().EstaEliminado);
    }

    [TestMethod]
    public void Registrar_CorreoDuplicado_ElErrorIndicaElCampoCorreo()
    {
        _servicio.Registrar(CrearDto());

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            _servicio.Registrar(CrearDto(dni: "11111111")));

        Assert.AreEqual("Correo", excepcion.Campo);
    }

    [TestMethod]
    public void Registrar_DatosValidos_GuardaLosDatosIngresados()
    {
        UsuarioDto dto = CrearDto();

        _servicio.Registrar(dto);

        Usuario? guardado = LeerUsuarioPorDni(dto.Dni);
        Assert.IsNotNull(guardado);
        Assert.AreEqual(dto.Nombre, guardado.Nombre);
        Assert.AreEqual(dto.Apellido, guardado.Apellido);
        Assert.AreEqual(dto.Correo, guardado.Correo);
    }

    [TestMethod]
    public void Registrar_DatosValidos_RegistraElAutorregistroConElUsuarioComoResponsable()
    {
        UsuarioDto dto = CrearDto();

        _servicio.Registrar(dto);

        RegistroAuditoria registro = LeerLog().Single();
        Assert.AreEqual(AccionAuditada.RegistroDeUsuario, registro.Accion);
        Assert.AreEqual(dto.Dni, registro.Responsable?.Dni);
    }

    [TestMethod]
    public void Registrar_DatosValidos_ElDetalleTieneElNombreYElDni()
    {
        _servicio.Registrar(CrearDto());

        Assert.AreEqual("Se registró Felipe Sosa (DNI 12345672)", LeerLog().Single().Detalle);
    }

    [TestMethod]
    public void Registrar_DniDuplicado_NoAgregaNadaAlLog()
    {
        _servicio.Registrar(CrearDto());

        Assert.ThrowsException<ServicioException>(() => _servicio.Registrar(CrearDto(correo: "otro@hotmail.com")));

        Assert.AreEqual(1, LeerLog().Count);
    }

    private Usuario? LeerUsuarioPorDni(string dni)
    {
        using SqlContext lectura = new SqlContext(_opciones);
        return new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorDni(dni);
    }

    private Usuario? LeerUsuarioPorCorreo(string correo)
    {
        using SqlContext lectura = new SqlContext(_opciones);
        return new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorCorreo(correo);
    }

    private IReadOnlyList<RegistroAuditoria> LeerLog()
    {
        using SqlContext lectura = new SqlContext(_opciones);
        return new RepositorioAuditoriaSql(lectura).ObtenerRegistros(new FiltroAuditoria { Desde = Ahora });
    }

    private static UsuarioDto CrearDto(string dni = "12345672", string correo = "felipe@hotmail.com")
    {
        return new UsuarioDto
        {
            Dni = dni, Nombre = "Felipe", Apellido = "Sosa", Correo = correo, Contrasena = "VivaLaOrt2026"
        };
    }
}