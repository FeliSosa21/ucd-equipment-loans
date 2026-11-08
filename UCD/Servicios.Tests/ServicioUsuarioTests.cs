using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioUsuarioTests
{
    [TestMethod]
    public void Registrar_DatosValidos_SeGuarda()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio, TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto dto = CrearDto();

        servicio.Registrar(dto);

        Assert.IsNotNull(repositorio.ObtenerUsuarioPorDni(dto.Dni));
    }

    [TestMethod]
    public void Registrar_DatosValidos_LeAsignaSinPermisosDelRepositorio()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        RepositorioTipoUsuario repositorioTipos = TiposDePrueba.Cargados(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio, repositorioTipos,
            AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto dto = CrearDto();

        servicio.Registrar(dto);

        TipoUsuario sinPermisos = repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.SinPermisos().Nombre)!;
        Assert.AreSame(sinPermisos, repositorio.ObtenerUsuarioPorDni(dto.Dni)!.Tipo);
    }

    [TestMethod]
    public void Registrar_DniDuplicado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio, TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto primero = CrearDto();
        UsuarioDto repetido = CrearDto(correo: "otro@hotmail.com");
        servicio.Registrar(primero);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(repetido));
        Assert.IsNull(repositorio.ObtenerUsuarioPorCorreo(repetido.Correo));
    }

    [TestMethod]
    public void Registrar_DniDuplicadoSinCeroAdelante_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio, TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto primero = CrearDto(dni: "01234561");
        UsuarioDto repetido = CrearDto(dni: "1234561", correo: "otro@hotmail.com");
        servicio.Registrar(primero);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(repetido));
        Assert.IsNull(repositorio.ObtenerUsuarioPorCorreo(repetido.Correo));
    }

    [TestMethod]
    public void Registrar_CorreoDuplicadoConOtrasMayusculas_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio, TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto primero = CrearDto();
        UsuarioDto repetido = CrearDto(dni: "45678905", correo: "Felipe@Hotmail.COM");
        servicio.Registrar(primero);

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(repetido));
        Assert.IsNull(repositorio.ObtenerUsuarioPorDni(repetido.Dni));
    }

    [TestMethod]
    public void Registrar_DniInvalido_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio, TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto dto = CrearDto(dni: "12345678");

        Assert.ThrowsException<ServicioException>(() => servicio.Registrar(dto));
    }

    [TestMethod]
    public void Registrar_DniInvalido_ElErrorIndicaElCampoDni()
    {
        Memoria memoria = new Memoria();
        ServicioUsuario servicio = new ServicioUsuario(new RepositorioUsuario(memoria), TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto dto = CrearDto(dni: "12345678");

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() => servicio.Registrar(dto));

        Assert.AreEqual("Dni", excepcion.Campo);
    }

    [TestMethod]
    public void Registrar_DniDuplicado_ElErrorIndicaElCampoDni()
    {
        Memoria memoria = new Memoria();
        ServicioUsuario servicio = new ServicioUsuario(new RepositorioUsuario(memoria), TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        servicio.Registrar(CrearDto());

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(CrearDto(correo: "otro@hotmail.com")));

        Assert.AreEqual("Dni", excepcion.Campo);
    }

    [TestMethod]
    public void Registrar_CorreoDuplicado_ElErrorIndicaElCampoCorreo()
    {
        Memoria memoria = new Memoria();
        ServicioUsuario servicio = new ServicioUsuario(new RepositorioUsuario(memoria), TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        servicio.Registrar(CrearDto());

        ServicioException excepcion = Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(CrearDto(dni: "11111111")));

        Assert.AreEqual("Correo", excepcion.Campo);
    }

    [TestMethod]
    public void Registrar_DatosValidos_GuardaLosDatosIngresados()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio, TiposDePrueba.Cargados(memoria),
            AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto dto = CrearDto();

        servicio.Registrar(dto);

        Usuario? guardado = repositorio.ObtenerUsuarioPorDni(dto.Dni);
        Assert.IsNotNull(guardado);
        Assert.AreEqual(dto.Nombre, guardado.Nombre);
        Assert.AreEqual(dto.Apellido, guardado.Apellido);
        Assert.AreEqual(dto.Correo, guardado.Correo);
    }

    [TestMethod]
    public void Registrar_DatosValidos_RegistraElAutorregistroConElUsuarioComoResponsable()
    {
        Memoria memoria = new Memoria();
        ServicioUsuario servicio = new ServicioUsuario(new RepositorioUsuario(memoria),
            TiposDePrueba.Cargados(memoria), AuditoriaDePrueba.Registrador(memoria));
        UsuarioDto dto = CrearDto();

        servicio.Registrar(dto);

        RegistroAuditoria registro = LeerLog(memoria).Single();
        Assert.AreEqual(AccionAuditada.RegistroDeUsuario, registro.Accion);
        Assert.AreEqual(dto.Dni, registro.Responsable?.Dni);
    }

    private static IReadOnlyList<RegistroAuditoria> LeerLog(Memoria memoria)
    {
        return new RepositorioAuditoria(memoria).ObtenerRegistros(
            new FiltroAuditoria { Desde = AuditoriaDePrueba.Ahora });
    }

    private static UsuarioDto CrearDto(string dni = "12345672", string correo = "felipe@hotmail.com")
    {
        return new UsuarioDto
        {
            Dni = dni, Nombre = "Felipe", Apellido = "Sosa", Correo = correo, Contrasena = "VivaLaOrt2026"
        };
    }
}