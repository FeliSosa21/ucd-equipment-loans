using Data;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

[TestClass]
public class ServicioTiposUsuarioTests
{
    private const string DniAdministrador = "11111111";
    private const string DniEstudiante = "12345672";
    private const string ImagenPng = "data:image/png;base64,iVBORw0KGgo=";
    private const string ImagenJpeg = "data:image/jpeg;base64,/9j/4AAQ";

    private DbContextOptions<SqlContext> _opciones = null!;
    private SqlContext _contexto = null!;
    private RepositorioUsuarioSql _repositorioUsuarios = null!;
    private RepositorioTipoUsuarioSql _repositorioTipos = null!;
    private ServicioTiposUsuario _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (SqlContext preparacion = new SqlContext(_opciones))
        {
            preparacion.Database.EnsureCreated();
            Usuario administrador = new Usuario(dni: DniAdministrador, nombre: "Root", apellido: "Admin",
                correo: "root@ucd.edu.uy", contrasena: "Root2026");
            administrador.AsignarTipo(new RepositorioTipoUsuarioSql(preparacion)
                .ObtenerTipoPorNombre(TipoUsuario.Administrador().Nombre)!);
            new RepositorioUsuarioSql(preparacion).AgregarUsuario(administrador);
        }

        _contexto = new SqlContext(_opciones);
        _repositorioUsuarios = new RepositorioUsuarioSql(_contexto);
        _repositorioTipos = new RepositorioTipoUsuarioSql(_contexto);
        _servicio = new ServicioTiposUsuario(_repositorioTipos, _repositorioUsuarios);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void ListarTipos_SeisTiposCargados_DevuelveSusNombres()
    {
        List<TipoUsuarioDto> tipos = _servicio.ListarTipos(DniAdministrador);

        CollectionAssert.AreEqual(TipoUsuario.Todos().Select(tipo => tipo.Nombre).ToList(),
            tipos.Select(tipo => tipo.Nombre).ToList());
    }

    [TestMethod]
    public void ListarTipos_SeisTiposCargados_DevuelveLosTopesDeCadaTipo()
    {
        List<TipoUsuarioDto> tipos = _servicio.ListarTipos(DniAdministrador);

        TipoUsuarioDto docente = tipos.Single(tipo => tipo.Nombre == TipoUsuario.Docente().Nombre);
        TipoUsuarioDto administrador = tipos.Single(tipo => tipo.Nombre == TipoUsuario.Administrador().Nombre);
        Assert.AreEqual(40, docente.MaxUnidadesPorCatalogo);
        Assert.AreEqual(3, docente.MaxPrestamosConcurrentes);
        Assert.IsNull(administrador.MaxUnidadesPorCatalogo);
        Assert.IsNull(administrador.MaxPrestamosConcurrentes);
    }

    [TestMethod]
    public void ListarTipos_ResponsableNoAdministrador_LanzaExcepcion()
    {
        AgregarEstudiante();

        Assert.ThrowsException<ServicioException>(() => _servicio.ListarTipos(DniEstudiante));
    }

    [TestMethod]
    public void ModificarTipo_DescripcionNueva_LaGuarda()
    {
        TipoUsuarioDto dto = CrearDtoDocente();
        dto.Descripcion = "Docentes de la facultad";

        _servicio.ModificarTipo(dto, DniAdministrador);

        Assert.AreEqual("Docentes de la facultad",
            _repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!.Descripcion);
    }

    [TestMethod]
    public void ModificarTipo_ImagenNueva_LaGuarda()
    {
        TipoUsuarioDto dto = CrearDtoDocente();
        dto.Imagen = ImagenJpeg;

        _servicio.ModificarTipo(dto, DniAdministrador);

        Assert.AreEqual(ImagenJpeg, _repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!.Imagen);
    }

    [TestMethod]
    public void ModificarTipo_TopesNuevos_LosGuarda()
    {
        TipoUsuarioDto dto = CrearDtoDocente();
        dto.MaxUnidadesPorCatalogo = 10;
        dto.MaxPrestamosConcurrentes = 2;

        _servicio.ModificarTipo(dto, DniAdministrador);

        TipoUsuario docente = _repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!;
        Assert.AreEqual(10, docente.MaxUnidadesPorCatalogo);
        Assert.AreEqual(2, docente.MaxPrestamosConcurrentes);
    }

    [TestMethod]
    public void ModificarTipo_AdministradorSinTopes_GuardaLaDescripcion()
    {
        TipoUsuarioDto dto = new TipoUsuarioDto
        {
            Nombre = TipoUsuario.Administrador().Nombre,
            Descripcion = "Gestiona usuarios y tipos",
            Imagen = ImagenPng
        };

        _servicio.ModificarTipo(dto, DniAdministrador);

        Assert.AreEqual("Gestiona usuarios y tipos",
            _repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Administrador().Nombre)!.Descripcion);
    }

    [TestMethod]
    public void ModificarTipo_TipoInexistente_LanzaExcepcion()
    {
        TipoUsuarioDto dto = CrearDtoDocente();
        dto.Nombre = "Invitado";

        Assert.ThrowsException<ServicioException>(() => _servicio.ModificarTipo(dto, DniAdministrador));
    }

    [TestMethod]
    public void ModificarTipo_ResponsableNoAdministrador_LanzaExcepcion()
    {
        AgregarEstudiante();

        Assert.ThrowsException<ServicioException>(() => _servicio.ModificarTipo(CrearDtoDocente(), DniEstudiante));
    }

    [TestMethod]
    public void ModificarTipo_DescripcionVacia_LanzaServicioException()
    {
        TipoUsuarioDto dto = CrearDtoDocente();
        dto.Descripcion = "";

        Assert.ThrowsException<ServicioException>(() => _servicio.ModificarTipo(dto, DniAdministrador));
    }

    [TestMethod]
    public void ModificarTipo_ImagenInvalida_NoCambiaLaDescripcion()
    {
        _servicio.ModificarTipo(CrearDtoDocente(), DniAdministrador);
        TipoUsuarioDto dto = CrearDtoDocente();
        dto.Descripcion = "Docentes de la facultad";
        dto.Imagen = "imagen invalida";

        Assert.ThrowsException<ServicioException>(() => _servicio.ModificarTipo(dto, DniAdministrador));

        Assert.AreEqual("Docente", _repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!.Descripcion);
    }

    [TestMethod]
    public void ModificarTipo_TopesInvalidos_NoCambiaLaDescripcion()
    {
        _servicio.ModificarTipo(CrearDtoDocente(), DniAdministrador);
        TipoUsuarioDto dto = CrearDtoDocente();
        dto.Descripcion = "Docentes de la facultad";
        dto.MaxUnidadesPorCatalogo = -1;

        Assert.ThrowsException<ServicioException>(() => _servicio.ModificarTipo(dto, DniAdministrador));

        Assert.AreEqual("Docente", _repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!.Descripcion);
    }

    [TestMethod]
    public void ModificarTipo_ImagenInvalida_NoCambiaLosTopes()
    {
        TipoUsuarioDto dto = CrearDtoDocente();
        dto.MaxUnidadesPorCatalogo = 10;
        dto.Imagen = "imagen invalida";

        Assert.ThrowsException<ServicioException>(() => _servicio.ModificarTipo(dto, DniAdministrador));

        Assert.AreEqual(40, _repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!.MaxUnidadesPorCatalogo);
    }

    private void AgregarEstudiante()
    {
        Usuario estudiante = new Usuario(dni: DniEstudiante, nombre: "Juan", apellido: "Gomez",
            correo: "juan@hotmail.com", contrasena: "VivaLaOrt2026");
        estudiante.AsignarTipo(_repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Estudiante().Nombre)!);
        _repositorioUsuarios.AgregarUsuario(estudiante);
    }

    private static TipoUsuarioDto CrearDtoDocente()
    {
        return new TipoUsuarioDto
        {
            Nombre = TipoUsuario.Docente().Nombre,
            Descripcion = "Docente",
            Imagen = ImagenPng,
            MaxUnidadesPorCatalogo = 40,
            MaxPrestamosConcurrentes = 3
        };
    }
}
