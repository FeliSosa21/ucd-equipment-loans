using Data;
using Data.Repositorios;
using Dominio;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

[TestClass]
public class ServicioAdministracionUsuariosTests
{
    private const string DniValido = "12345672";
    private const string CorreoNuevo = "ana@hotmail.com";
    private const string DniResponsable = "11111111";
    private const string DniNuevo = "45678905";

    private static readonly DateTime Ahora = new(2026, 11, 8, 9, 30, 0);
    private DbContextOptions<SqlContext> _opciones = null!;
    private SqlContext _contexto = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _contexto = new SqlContext(_opciones);
        _contexto.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Limpiar()
    {
        _contexto.Dispose();
    }

    [TestMethod]
    public void ListarUsuarios_FiltroConTexto_DevuelveLosQueCoincidenConSuTipoYEstado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<UsuarioResumenDto> usuarios =
            servicio.ListarUsuarios(new FiltroUsuariosDto { Texto = "sosa" }, DniResponsable);

        UsuarioResumenDto registrado = usuarios.Single();
        Assert.AreEqual(DniValido, registrado.Dni);
        Assert.AreEqual(TipoUsuario.SinPermisos().Nombre, registrado.Tipo);
        Assert.IsTrue(registrado.Activo);
    }

    [TestMethod]
    public void ListarUsuarios_FiltroConNombreDeTipo_DevuelveSoloLosDeEseTipo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<UsuarioResumenDto> usuarios = servicio.ListarUsuarios(
            new FiltroUsuariosDto { NombreTipo = TipoUsuario.SinPermisos().Nombre }, DniResponsable);

        CollectionAssert.AreEqual(new List<string> { DniValido },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ListarUsuarios_TipoInexistente_LanzaServicioException()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarUsuarios(new FiltroUsuariosDto { NombreTipo = "Rector" }, DniResponsable));

        Assert.AreEqual("El tipo de usuario no existe", excepcion.Message);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void ListarUsuarios_TextoYNombreDeTipoVacio_FiltraSoloPorTexto(string nombreTipo)
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<UsuarioResumenDto> usuarios = servicio.ListarUsuarios(
            new FiltroUsuariosDto { Texto = "sosa", NombreTipo = nombreTipo }, DniResponsable);

        CollectionAssert.AreEqual(new List<string> { DniValido },
            usuarios.Select(usuario => usuario.Dni).ToList());
    }

    [TestMethod]
    public void ListarUsuarios_FiltroSinCriterios_DevuelveListaVacia()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<UsuarioResumenDto> usuarios = servicio.ListarUsuarios(new FiltroUsuariosDto(), DniResponsable);

        Assert.AreEqual(0, usuarios.Count);
    }

    [TestMethod]
    public void ListarUsuarios_FiltroNulo_LanzaArgumentNullException()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ListarUsuarios(null!, DniResponsable));
    }

    [TestMethod]
    public void ListarUsuarios_ResponsableNoAdministrador_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarUsuarios(new FiltroUsuariosDto { Texto = "sosa" }, DniValido));
    }

    [TestMethod]
    public void TieneCriterios_ConTexto_DevuelveTrue()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        bool tieneCriterios = servicio.TieneCriterios(new FiltroUsuariosDto { Texto = "sosa" });

        Assert.IsTrue(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_SinTextoNiTipo_DevuelveFalse()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        bool tieneCriterios = servicio.TieneCriterios(new FiltroUsuariosDto());

        Assert.IsFalse(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_ConNombreDeTipo_DevuelveTrue()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        bool tieneCriterios = servicio.TieneCriterios(
            new FiltroUsuariosDto { NombreTipo = ServicioAdministracionUsuarios.NombreTipoPendiente });

        Assert.IsTrue(tieneCriterios);
    }

    [TestMethod]
    public void TieneCriterios_TextoYTipoEnBlanco_DevuelveFalse()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        bool tieneCriterios = servicio.TieneCriterios(new FiltroUsuariosDto { Texto = "   ", NombreTipo = "" });

        Assert.IsFalse(tieneCriterios);
    }

    [TestMethod]
    public void NombresDeTipos_ConNombreTipoAdministrador_LoIncluyeEntreLosAsignables()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<string> nombres = servicio.NombresDeTipos();

        CollectionAssert.Contains(nombres, ServicioAdministracionUsuarios.NombreTipoAdministrador);
    }

    [TestMethod]
    public void NombresDeTipos_ConNombreTipoPendiente_LoIncluyeEntreLosAsignables()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<string> nombres = servicio.NombresDeTipos();

        CollectionAssert.Contains(nombres, ServicioAdministracionUsuarios.NombreTipoPendiente);
    }

    [TestMethod]
    public void NombresDeTipos_SinFiltro_DevuelveLosSeisTipos()
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);

        servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniResponsable);

        Assert.AreEqual(TipoUsuario.Docente().Nombre, LeerUsuarioPorDni(DniValido)!.Tipo.Nombre);
    }

    [TestMethod]
    public void AsignarTipo_TipoDelRepositorio_GuardaSuIdentificador()
    {
        ServicioAdministracionUsuarios servicio =
            CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio, out RepositorioTipoUsuarioSql repositorioTipos);
        TipoUsuario docente = repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!;

        servicio.AsignarTipo(DniValido, docente.Nombre, DniResponsable);

        Assert.AreEqual(docente.Id, LeerUsuarioPorDni(DniValido)!.Tipo.Id);
    }

    [TestMethod]
    public void AsignarTipo_AuditoriaConOtroContexto_PersisteElTipoAsignado()
    {
        CrearServicioConAdministrador(out RepositorioUsuarioSql usuarios, out RepositorioTipoUsuarioSql tipos);
        using SqlContext contextoAuditoria = new SqlContext(_opciones);
        contextoAuditoria.Attach(usuarios.ObtenerUsuarioPorDni(DniResponsable)!);
        RegistradorDeAuditoria auditoria = new RegistradorDeAuditoria(
            new RepositorioAuditoriaSql(contextoAuditoria), new RelojFijo(Ahora));
        ServicioUsuario servicioUsuario = new ServicioUsuario(usuarios, tipos, auditoria);
        ServicioAdministracionUsuarios servicio = new ServicioAdministracionUsuarios(
            usuarios, servicioUsuario, tipos, auditoria);
        TipoUsuario docente = tipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!;

        servicio.AsignarTipo(DniValido, docente.Nombre, DniResponsable);

        using SqlContext lectura = new SqlContext(_opciones);
        Usuario guardado = new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorDni(DniValido)!;
        Assert.AreEqual(docente.Id, guardado.Tipo.Id);
        Assert.AreEqual(docente.Nombre, guardado.Tipo.Nombre);
        Assert.AreEqual(AccionAuditada.CambioDeTipoDeUsuario, LeerAccionesDelAdministrador().Single().Accion);
    }

    [TestMethod]
    public void AsignarTipo_TipoExistente_RegistraElCambioConElAdministradorComoResponsable()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniResponsable);

        Assert.AreEqual(AccionAuditada.CambioDeTipoDeUsuario,
            LeerAccionesDelAdministrador().Single().Accion);
    }

    [TestMethod]
    public void AsignarTipo_TipoExistente_ElDetalleTieneElUsuarioYElTipoAnteriorYElNuevo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniResponsable);

        Assert.AreEqual("Felipe Sosa (DNI 12345672) pasó de Sin permisos a Docente",
            LeerAccionesDelAdministrador().Single().Detalle);
    }

    [TestMethod]
    public void AsignarTipo_MismoTipo_NoAgregaNadaAlLog()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        servicio.AsignarTipo(DniValido, TipoUsuario.SinPermisos().Nombre, DniResponsable);

        Assert.AreEqual(0, LeerAccionesDelAdministrador().Count);
    }

    [TestMethod]
    public void AsignarTipo_TipoInexistente_NoAgregaNadaAlLog()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        Assert.ThrowsException<ServicioException>(() => servicio.AsignarTipo(DniValido, "Rector", DniResponsable));

        Assert.AreEqual(0, LeerAccionesDelAdministrador().Count);
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
            servicio.AsignarTipo(DniNuevo, TipoUsuario.Docente().Nombre, DniResponsable));
    }

    [TestMethod]
    public void Desactivar_UsuarioActivo_QuedaDesactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);

        servicio.Desactivar(DniValido, DniResponsable);

        Assert.AreEqual(EstadoUsuario.Desactivado, LeerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Desactivar_AuditoriaConOtroContexto_PersisteElEstadoDesactivado()
    {
        CrearServicioConAdministrador(out RepositorioUsuarioSql usuarios, out RepositorioTipoUsuarioSql tipos);
        using SqlContext contextoAuditoria = new SqlContext(_opciones);
        contextoAuditoria.Attach(usuarios.ObtenerUsuarioPorDni(DniResponsable)!);
        RegistradorDeAuditoria auditoria = new RegistradorDeAuditoria(
            new RepositorioAuditoriaSql(contextoAuditoria), new RelojFijo(Ahora));
        ServicioUsuario servicioUsuario = new ServicioUsuario(usuarios, tipos, auditoria);
        ServicioAdministracionUsuarios servicio = new ServicioAdministracionUsuarios(
            usuarios, servicioUsuario, tipos, auditoria);

        servicio.Desactivar(DniValido, DniResponsable);

        using SqlContext lectura = new SqlContext(_opciones);
        Usuario guardado = new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorDni(DniValido)!;
        Assert.AreEqual(EstadoUsuario.Desactivado, guardado.Estado);
        Assert.AreEqual(AccionAuditada.DesactivacionDeUsuario, LeerAccionesDelAdministrador().Single().Accion);
    }

    [TestMethod]
    public void Desactivar_UsuarioActivo_RegistraLaDesactivacionConElAdministradorComoResponsable()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        servicio.Desactivar(DniValido, DniResponsable);

        Assert.AreEqual(AccionAuditada.DesactivacionDeUsuario,
            LeerAccionesDelAdministrador().Single().Accion);
    }

    [TestMethod]
    public void Desactivar_UsuarioActivo_ElDetalleTieneElUsuarioDesactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        servicio.Desactivar(DniValido, DniResponsable);

        Assert.AreEqual("Se desactivó la cuenta de Felipe Sosa (DNI 12345672)",
            LeerAccionesDelAdministrador().Single().Detalle);
    }

    [TestMethod]
    public void Desactivar_UsuarioInexistente_NoAgregaNadaAlLog()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniNuevo, DniResponsable));

        Assert.AreEqual(0, LeerAccionesDelAdministrador().Count);
    }

    [TestMethod]
    public void Desactivar_UsuarioYaDesactivado_NoAgregaNadaAlLog()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);
        servicio.Desactivar(DniValido, DniResponsable);

        servicio.Desactivar(DniValido, DniResponsable);

        Assert.AreEqual(1, LeerAccionesDelAdministrador().Count);
    }

    [TestMethod]
    public void Reactivar_UsuarioDesactivado_QuedaActivo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.Desactivar(DniValido, DniResponsable);

        servicio.Reactivar(DniValido, DniResponsable);

        Assert.AreEqual(EstadoUsuario.Activo, LeerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Reactivar_AuditoriaConOtroContexto_PersisteElEstadoActivo()
    {
        CrearServicioConAdministrador(out RepositorioUsuarioSql usuarios, out RepositorioTipoUsuarioSql tipos);
        Usuario usuario = usuarios.ObtenerUsuarioPorDni(DniValido)!;
        usuario.Desactivar();
        usuarios.ActualizarUsuario(usuario);
        using SqlContext contextoAuditoria = new SqlContext(_opciones);
        contextoAuditoria.Attach(usuarios.ObtenerUsuarioPorDni(DniResponsable)!);
        RegistradorDeAuditoria auditoria = new RegistradorDeAuditoria(
            new RepositorioAuditoriaSql(contextoAuditoria), new RelojFijo(Ahora));
        ServicioUsuario servicioUsuario = new ServicioUsuario(usuarios, tipos, auditoria);
        ServicioAdministracionUsuarios servicio = new ServicioAdministracionUsuarios(
            usuarios, servicioUsuario, tipos, auditoria);

        servicio.Reactivar(DniValido, DniResponsable);

        using SqlContext lectura = new SqlContext(_opciones);
        Usuario guardado = new RepositorioUsuarioSql(lectura).ObtenerUsuarioPorDni(DniValido)!;
        Assert.AreEqual(EstadoUsuario.Activo, guardado.Estado);
        Assert.AreEqual(AccionAuditada.ReactivacionDeUsuario, LeerAccionesDelAdministrador().Single().Accion);
    }

    [TestMethod]
    public void Reactivar_UsuarioDesactivado_RegistraLaReactivacionConElAdministradorComoResponsable()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);
        servicio.Desactivar(DniValido, DniResponsable);

        servicio.Reactivar(DniValido, DniResponsable);

        Assert.AreEqual(1, LeerAccionesDelAdministrador()
            .Count(registro => registro.Accion == AccionAuditada.ReactivacionDeUsuario));
    }

    [TestMethod]
    public void Reactivar_UsuarioDesactivado_ElDetalleTieneElUsuarioReactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);
        servicio.Desactivar(DniValido, DniResponsable);

        servicio.Reactivar(DniValido, DniResponsable);

        RegistroAuditoria reactivacion = LeerAccionesDelAdministrador()
            .Single(registro => registro.Accion == AccionAuditada.ReactivacionDeUsuario);
        Assert.AreEqual("Se reactivó la cuenta de Felipe Sosa (DNI 12345672)", reactivacion.Detalle);
    }

    [TestMethod]
    public void Reactivar_UsuarioInexistente_NoAgregaNadaAlLog()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(DniNuevo, DniResponsable));

        Assert.AreEqual(0, LeerAccionesDelAdministrador().Count);
    }

    [TestMethod]
    public void Reactivar_UsuarioYaActivo_NoAgregaNadaAlLog()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        servicio.Reactivar(DniValido, DniResponsable);

        Assert.AreEqual(0, LeerAccionesDelAdministrador().Count);
    }

    [TestMethod]
    public void CrearUsuario_DatosValidos_QuedaConElTipoYLaContrasenaPredeterminada()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);

        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Usuario? creado = LeerUsuarioPorCorreo(CorreoNuevo);
        Assert.IsNotNull(creado);
        Assert.AreEqual(TipoUsuario.Estudiante().Nombre, creado.Tipo.Nombre);
        Assert.IsTrue(creado.ContrasenaEsCorrecta(ServicioAdministracionUsuarios.ContrasenaPredeterminada));
        Assert.IsTrue(creado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CrearUsuario_AuditoriaConOtroContexto_PersisteElTipoYElCambioObligatorio()
    {
        CrearServicioConAdministrador(out RepositorioUsuarioSql usuarios, out RepositorioTipoUsuarioSql tipos);
        using SqlContext contextoAuditoria = new SqlContext(_opciones);
        contextoAuditoria.Attach(usuarios.ObtenerUsuarioPorDni(DniResponsable)!);
        RegistradorDeAuditoria auditoria = new RegistradorDeAuditoria(
            new RepositorioAuditoriaSql(contextoAuditoria), new RelojFijo(Ahora));
        ServicioUsuario servicioUsuario = new ServicioUsuario(usuarios, tipos, auditoria);
        ServicioAdministracionUsuarios servicio = new ServicioAdministracionUsuarios(
            usuarios, servicioUsuario, tipos, auditoria);

        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Usuario guardado = LeerUsuarioPorDni(DniNuevo)!;
        Assert.AreEqual(TipoUsuario.Estudiante().Nombre, guardado.Tipo.Nombre);
        Assert.IsTrue(guardado.DebeCambiarContrasena);
        Assert.IsTrue(guardado.ContrasenaEsCorrecta(ServicioAdministracionUsuarios.ContrasenaPredeterminada));
        Assert.AreEqual(AccionAuditada.AltaDeUsuario, LeerAccionesDelAdministrador().Single().Accion);
    }

    [TestMethod]
    public void CrearUsuario_DatosValidos_RegistraElAltaConElAdministradorComoResponsable()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.AreEqual(AccionAuditada.AltaDeUsuario, LeerAccionesDelAdministrador().Single().Accion);
    }

    [TestMethod]
    public void CrearUsuario_DatosValidos_ElDetalleTieneElUsuarioCreadoYSuTipo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);

        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.AreEqual("Alta de Ana Perez (DNI 45678905) como Estudiante",
            LeerAccionesDelAdministrador().Single().Detalle);
    }

    [TestMethod]
    public void CrearUsuario_DniDuplicado_NoAgregaNadaAlLog()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _, out _);
        UsuarioDto dto = CrearDtoNuevo();
        dto.Dni = DniValido;

        Assert.ThrowsException<ServicioException>(() => servicio.CrearUsuario(dto, TipoUsuario.Estudiante().Nombre,
            DniResponsable));

        Assert.AreEqual(0, LeerAccionesDelAdministrador().Count);
    }

    [TestMethod]
    public void CrearUsuario_CedulaDe7Digitos_LeAsignaElTipo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        UsuarioDto dto = CrearDtoNuevo();
        dto.Dni = "1234561";

        servicio.CrearUsuario(dto, TipoUsuario.Docente().Nombre, DniResponsable);

        Usuario creado = LeerUsuarioPorDni("01234561")!;
        Assert.AreEqual(TipoUsuario.Docente().Nombre, creado.Tipo.Nombre);
        Assert.IsTrue(creado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CrearUsuario_ConEspaciosAlrededor_LosGuardaRecortados()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        UsuarioDto dto = CrearDtoNuevo();
        dto.Nombre = " Pedro ";
        dto.Apellido = "  Suarez ";
        dto.Correo = " " + CorreoNuevo + "  ";

        servicio.CrearUsuario(dto, TipoUsuario.Estudiante().Nombre, DniResponsable);

        Usuario creado = LeerUsuarioPorDni(DniNuevo)!;
        Assert.AreEqual("Pedro", creado.Nombre);
        Assert.AreEqual("Suarez", creado.Apellido);
        Assert.AreEqual(CorreoNuevo, creado.Correo);
        Assert.AreEqual(TipoUsuario.Estudiante().Nombre, creado.Tipo.Nombre);
    }

    [TestMethod]
    public void CrearUsuario_DniDuplicado_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        UsuarioDto dto = CrearDtoNuevo();
        dto.Dni = DniValido;

        Assert.ThrowsException<ServicioException>(() => servicio.CrearUsuario(dto, TipoUsuario.Estudiante().Nombre,
            DniResponsable));
    }

    [TestMethod]
    public void CrearUsuario_TipoInexistente_NoLoGuarda()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CrearUsuario(CrearDtoNuevo(), "Rector", DniResponsable));
        Assert.IsNull(LeerUsuarioPorCorreo(CorreoNuevo));
    }

    [TestMethod]
    public void ObtenerResumen_UsuarioExistente_DevuelveSusDatos()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        UsuarioResumenDto resumen = servicio.ObtenerResumen(DniValido, DniValido);

        Assert.AreEqual("Felipe", resumen.Nombre);
    }

    [TestMethod]
    public void ModificarDatos_DatosValidos_LosActualiza()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        servicio.ModificarDatos(
            CrearDatos(dni: DniValido, nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"), DniValido);

        Usuario usuario = LeerUsuarioPorDni(DniValido)!;
        Assert.AreEqual("Pedro", usuario.Nombre);
        Assert.AreEqual("pedro@hotmail.com", usuario.Correo);
    }

    [TestMethod]
    public void ModificarDatos_DatosValidos_PersisteNombreApellidoYCorreo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        DatosPersonalesDto datos = CrearDatos(dni: DniValido, nombre: "Pedro", apellido: "Suarez",
            correo: "pedro@hotmail.com");

        servicio.ModificarDatos(datos, DniValido);

        Usuario guardado = LeerUsuarioPorDni(DniValido)!;
        Assert.AreEqual(datos.Nombre, guardado.Nombre);
        Assert.AreEqual(datos.Apellido, guardado.Apellido);
        Assert.AreEqual(datos.Correo, guardado.Correo);
    }

    [TestMethod]
    public void ModificarDatos_MismoCorreoConOtrasMayusculas_LoPermite()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        servicio.ModificarDatos(
            CrearDatos(dni: DniValido, nombre: "Felipe", apellido: "Sosa", correo: "FELIPE@hotmail.com"), DniValido);

        Assert.AreEqual("FELIPE@hotmail.com", LeerUsuarioPorDni(DniValido)!.Correo);
    }

    [TestMethod]
    public void ModificarDatos_CorreoDeOtroUsuario_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: DniValido, nombre: "Felipe", apellido: "Sosa", correo: CorreoNuevo), DniValido));
    }

    [TestMethod]
    public void ModificarDatos_CorreoDeOtroUsuario_ElErrorIndicaElCampoCorreo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: DniValido, nombre: "Felipe", apellido: "Sosa", correo: CorreoNuevo), DniValido));

        Assert.AreEqual("Correo", excepcion.Campo);
    }

    [TestMethod]
    public void ModificarDatos_NombreInvalido_LanzaExcepcionYConservaLosDatosGuardados()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: DniValido, nombre: "F", apellido: "Suarez", correo: "pedro@hotmail.com"), DniValido));

        Usuario guardado = LeerUsuarioPorDni(DniValido)!;
        Assert.AreEqual("Felipe", guardado.Nombre);
        Assert.AreEqual("Sosa", guardado.Apellido);
        Assert.AreEqual("felipe@hotmail.com", guardado.Correo);
    }

    [TestMethod]
    public void Desactivar_OtroAdministrador_QuedaDesactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador().Nombre, DniResponsable);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador().Nombre, DniResponsable);

        servicio.Desactivar(DniValido, DniResponsable);

        Assert.AreEqual(EstadoUsuario.Desactivado, LeerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void AsignarTipo_AdministradorAAdministrador_LoMantiene()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador().Nombre, DniResponsable);

        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador().Nombre, DniResponsable);

        Assert.AreEqual(TipoUsuario.Administrador().Nombre, LeerUsuarioPorDni(DniValido)!.Tipo.Nombre);
    }

    [TestMethod]
    public void Desactivar_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniNuevo, DniResponsable));
    }

    [TestMethod]
    public void Reactivar_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(DniNuevo, DniResponsable));
    }

    [TestMethod]
    public void Reactivar_UsuarioEliminado_LanzaExcepcionDeUsuarioInexistente()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        repositorio.EliminarUsuario(repositorio.ObtenerUsuarioPorDni(DniValido)!);

        ServicioException excepcion =
            Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(DniValido, DniResponsable));

        Assert.AreEqual("El usuario no existe", excepcion.Message);
    }

    [TestMethod]
    public void ObtenerResumen_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.ObtenerResumen(DniNuevo, DniResponsable));
    }

    [TestMethod]
    public void ModificarDatos_UsuarioInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: DniNuevo, nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"),
                DniNuevo));
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
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniValido));
    }

    [TestMethod]
    public void ModificarDatos_AdministradorModificaAOtro_LosActualiza()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador().Nombre, DniResponsable);

        servicio.ModificarDatos(CrearDatos(dni: DniValido, nombre: "Pedro", apellido: "Suarez",
            correo: "pedro@hotmail.com"), DniNuevo);

        Assert.AreEqual("Pedro", LeerUsuarioPorDni(DniValido)!.Nombre);
    }

    [TestMethod]
    public void ModificarDatos_NoAdministradorModificaAOtro_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.ModificarDatos(CrearDatos(dni: DniValido,
            nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"), DniNuevo));
        Assert.AreEqual("Felipe", LeerUsuarioPorDni(DniValido)!.Nombre);
    }

    [TestMethod]
    public void CrearUsuario_DtoNulo_LanzaArgumentNullException()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ArgumentNullException>(() =>
            servicio.CrearUsuario(null!, TipoUsuario.Estudiante().Nombre, DniResponsable));
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniValido));
        Assert.IsNull(LeerUsuarioPorCorreo(CorreoNuevo));
    }

    [TestMethod]
    public void AsignarTipo_ResponsableNoAdministrador_NoLoCambia()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniNuevo));
        Assert.AreEqual(TipoUsuario.SinPermisos().Nombre, LeerUsuarioPorDni(DniValido)!.Tipo.Nombre);
    }

    [TestMethod]
    public void AsignarTipo_ResponsableDesactivado_NoLoCambia()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador().Nombre, DniResponsable);
        servicio.Desactivar(DniResponsable, DniNuevo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniResponsable));
        Assert.AreEqual(TipoUsuario.SinPermisos().Nombre, LeerUsuarioPorDni(DniValido)!.Tipo.Nombre);
    }

    [TestMethod]
    public void Desactivar_ResponsableDesactivado_NoLoDesactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador().Nombre, DniResponsable);
        servicio.Desactivar(DniResponsable, DniNuevo);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniValido, DniResponsable));
        Assert.AreEqual(EstadoUsuario.Activo, LeerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Reactivar_ResponsableNoAdministrador_NoLoReactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);
        servicio.Desactivar(DniValido, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(DniValido, DniNuevo));
        Assert.AreEqual(EstadoUsuario.Desactivado, LeerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Desactivar_UnicoAdministradorActivoASiMismo_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniResponsable, DniResponsable));
        Assert.AreEqual(EstadoUsuario.Activo, LeerUsuarioPorDni(DniResponsable)!.Estado);
    }

    [TestMethod]
    public void ObtenerResumen_OtroUsuarioSinSerAdministrador_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.ObtenerResumen(DniValido, DniNuevo));
    }

    [TestMethod]
    public void ObtenerResumen_UsuarioDesactivadoPideLosSuyos_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);
        servicio.Desactivar(DniValido, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.ObtenerResumen(DniValido, DniValido));
    }

    [TestMethod]
    public void ModificarDatos_UsuarioDesactivadoModificaLosSuyos_LanzaExcepcionYNoLosCambia()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.Desactivar(DniValido, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.ModificarDatos(CrearDatos(dni: DniValido,
            nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"), DniValido));
        Assert.AreEqual("Felipe", LeerUsuarioPorDni(DniValido)!.Nombre);
    }

    [TestMethod]
    public void ModificarDatos_ResponsableInexistente_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.ModificarDatos(CrearDatos(dni: DniValido,
            nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"), DniNuevo));
    }

    [TestMethod]
    public void ModificarDatos_ConEspaciosAlrededor_LosGuardaRecortados()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        servicio.ModificarDatos(CrearDatos(dni: DniValido, nombre: "  Pedro ", apellido: " Suarez  ",
            correo: " pedro@hotmail.com "), DniValido);

        Usuario usuario = LeerUsuarioPorDni(DniValido)!;
        Assert.AreEqual("Pedro", usuario.Nombre);
        Assert.AreEqual("Suarez", usuario.Apellido);
        Assert.AreEqual("pedro@hotmail.com", usuario.Correo);
    }

    [TestMethod]
    public void ModificarDatos_NombreDeUnCaracterConEspacios_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.ModificarDatos(CrearDatos(dni: DniValido,
            nombre: "  P  ", apellido: "Sosa", correo: "felipe@hotmail.com"), DniValido));
    }

    [TestMethod]
    public void Desactivar_ResponsableNoAdministrador_NoLoDesactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniValido, DniNuevo));
        Assert.AreEqual(EstadoUsuario.Activo, LeerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Reactivar_ResponsableInexistente_NoLoReactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.Desactivar(DniValido, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(DniValido, DniNuevo));
        Assert.AreEqual(EstadoUsuario.Desactivado, LeerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void CrearUsuario_ResponsableDesactivado_NoLoGuarda()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador().Nombre, DniResponsable);
        servicio.Desactivar(DniResponsable, DniValido);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable));
        Assert.IsNull(LeerUsuarioPorCorreo(CorreoNuevo));
    }

    private static DatosPersonalesDto CrearDatos(string dni, string nombre, string apellido, string correo)
    {
        return new DatosPersonalesDto { Dni = dni, Nombre = nombre, Apellido = apellido, Correo = correo };
    }

    private static UsuarioDto CrearDtoNuevo()
    {
        return new UsuarioDto
        {
            Dni = DniNuevo, Nombre = "Ana", Apellido = "Perez", Correo = CorreoNuevo
        };
    }

    private ServicioAdministracionUsuarios CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio)
    {
        return CrearServicioConAdministrador(out repositorio, out _);
    }

    private ServicioAdministracionUsuarios CrearServicioConAdministrador(out RepositorioUsuarioSql repositorio,
        out RepositorioTipoUsuarioSql repositorioTipos)
    {
        repositorio = new RepositorioUsuarioSql(_contexto);
        repositorioTipos = new RepositorioTipoUsuarioSql(_contexto);
        RegistradorDeAuditoria auditoria = new RegistradorDeAuditoria(
            new RepositorioAuditoriaSql(_contexto), new RelojFijo(Ahora));
        ServicioUsuario servicioUsuario = new ServicioUsuario(repositorio, repositorioTipos, auditoria);
        servicioUsuario.Registrar(new UsuarioDto
        {
            Dni = DniValido, Nombre = "Felipe", Apellido = "Sosa", Correo = "felipe@hotmail.com",
            Contrasena = "VivaLaOrt2026"
        });
        Usuario administrador = new Usuario(dni: DniResponsable, nombre: "Root", apellido: "Admin",
            correo: "root@ucd.edu.uy", contrasena: "Root2026");
        administrador.AsignarTipo(repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Administrador().Nombre)!);
        repositorio.AgregarUsuario(administrador);
        return new ServicioAdministracionUsuarios(repositorio, servicioUsuario, repositorioTipos, auditoria);
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

    private IReadOnlyList<RegistroAuditoria> LeerAccionesDelAdministrador()
    {
        using SqlContext lectura = new SqlContext(_opciones);
        return new RepositorioAuditoriaSql(lectura).ObtenerRegistros(
            new FiltroAuditoria { DniUsuario = DniResponsable });
    }
}
