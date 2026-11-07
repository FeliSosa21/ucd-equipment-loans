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

        List<UsuarioResumenDto> usuarios = servicio.ListarUsuarios(DniResponsable, null, 1).Elementos;

        UsuarioResumenDto registrado = usuarios.Single(usuario => usuario.Dni == DniValido);
        Assert.AreEqual(2, usuarios.Count);
        Assert.AreEqual(TipoUsuario.SinPermisos().Nombre, registrado.Tipo);
        Assert.IsTrue(registrado.Activo);
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
    public void ListarUsuarios_PocosUsuarios_DevuelveUnaSolaPagina()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        PaginaDto<UsuarioResumenDto> pagina = servicio.ListarUsuarios(DniResponsable, null, 1);

        Assert.AreEqual(2, pagina.Elementos.Count);
        Assert.IsFalse(pagina.HayPaginaAnterior);
        Assert.IsFalse(pagina.HayPaginaSiguiente);
    }

    [TestMethod]
    public void ListarUsuarios_MasUsuariosQueUnaPagina_LaPrimeraIndicaQueHaySiguiente()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        AgregarUsuarios(repositorio, ServicioAdministracionUsuarios.TamanoPagina);

        PaginaDto<UsuarioResumenDto> pagina = servicio.ListarUsuarios(DniResponsable, null, 1);

        Assert.AreEqual(ServicioAdministracionUsuarios.TamanoPagina, pagina.Elementos.Count);
        Assert.IsTrue(pagina.HayPaginaSiguiente);
    }

    [TestMethod]
    public void ListarUsuarios_UltimaPagina_IndicaQueHayAnteriorYNoSiguiente()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        AgregarUsuarios(repositorio, ServicioAdministracionUsuarios.TamanoPagina);

        PaginaDto<UsuarioResumenDto> pagina = servicio.ListarUsuarios(DniResponsable, null, 2);

        Assert.AreEqual(2, pagina.Elementos.Count);
        Assert.IsTrue(pagina.HayPaginaAnterior);
        Assert.IsFalse(pagina.HayPaginaSiguiente);
    }

    [TestMethod]
    public void ListarUsuarios_PaginaMenorAUno_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            servicio.ListarUsuarios(DniResponsable, null, 0));
    }

    [TestMethod]
    public void NombresDeTipos_ConNombreTipoAdministrador_LoIncluyeEntreLosAsignables()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        List<string> nombres = servicio.NombresDeTipos();

        CollectionAssert.Contains(nombres, ServicioAdministracionUsuarios.NombreTipoAdministrador);
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniResponsable);

        Assert.AreEqual(TipoUsuario.Docente().Nombre, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo.Nombre);
    }

    [TestMethod]
    public void AsignarTipo_TipoDelRepositorio_AsignaLaMismaInstancia()
    {
        ServicioAdministracionUsuarios servicio =
            CrearServicioConAdministrador(out RepositorioUsuario repositorio, out RepositorioTipoUsuario repositorioTipos);
        TipoUsuario docente = repositorioTipos.ObtenerTipoPorNombre(TipoUsuario.Docente().Nombre)!;

        servicio.AsignarTipo(DniValido, docente.Nombre, DniResponsable);

        Assert.AreSame(docente, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo);
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

        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Usuario? creado = repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo);
        Assert.IsNotNull(creado);
        Assert.AreEqual(TipoUsuario.Estudiante().Nombre, creado.Tipo.Nombre);
        Assert.IsTrue(creado.ContrasenaEsCorrecta(ServicioAdministracionUsuarios.ContrasenaPredeterminada));
        Assert.IsTrue(creado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CrearUsuario_CedulaDe7Digitos_LeAsignaElTipo()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        UsuarioDto dto = CrearDtoNuevo();
        dto.Dni = "1234561";

        servicio.CrearUsuario(dto, TipoUsuario.Docente().Nombre, DniResponsable);

        Usuario creado = repositorio.ObtenerUsuarioPorDni("01234561")!;
        Assert.AreEqual(TipoUsuario.Docente().Nombre, creado.Tipo.Nombre);
        Assert.IsTrue(creado.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CrearUsuario_ConEspaciosAlrededor_LosGuardaRecortados()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        UsuarioDto dto = CrearDtoNuevo();
        dto.Nombre = " Pedro ";
        dto.Apellido = "  Suarez ";
        dto.Correo = " " + CorreoNuevo + "  ";

        servicio.CrearUsuario(dto, TipoUsuario.Estudiante().Nombre, DniResponsable);

        Usuario creado = repositorio.ObtenerUsuarioPorDni(DniNuevo)!;
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CrearUsuario(CrearDtoNuevo(), "Rector", DniResponsable));
        Assert.IsNull(repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo));
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
    public void ModificarDatos_NombreInvalido_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ModificarDatos(
                CrearDatos(dni: DniValido, nombre: "F", apellido: "Sosa", correo: "felipe@hotmail.com"), DniValido));
    }

    [TestMethod]
    public void Desactivar_OtroAdministrador_QuedaDesactivado()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador().Nombre, DniResponsable);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador().Nombre, DniResponsable);

        servicio.Desactivar(DniValido, DniResponsable);

        Assert.AreEqual(EstadoUsuario.Desactivado, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void AsignarTipo_AdministradorAAdministrador_LoMantiene()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador().Nombre, DniResponsable);

        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador().Nombre, DniResponsable);

        Assert.AreEqual(TipoUsuario.Administrador().Nombre, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo.Nombre);
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador().Nombre, DniResponsable);

        servicio.ModificarDatos(CrearDatos(dni: DniValido, nombre: "Pedro", apellido: "Suarez",
            correo: "pedro@hotmail.com"), DniNuevo);

        Assert.AreEqual("Pedro", repositorio.ObtenerUsuarioPorDni(DniValido)!.Nombre);
    }

    [TestMethod]
    public void ModificarDatos_NoAdministradorModificaAOtro_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.ModificarDatos(CrearDatos(dni: DniValido,
            nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"), DniNuevo));
        Assert.AreEqual("Felipe", repositorio.ObtenerUsuarioPorDni(DniValido)!.Nombre);
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniValido));
        Assert.IsNull(repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo));
    }

    [TestMethod]
    public void AsignarTipo_ResponsableNoAdministrador_NoLoCambia()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniNuevo));
        Assert.AreEqual(TipoUsuario.SinPermisos().Nombre, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo.Nombre);
    }

    [TestMethod]
    public void AsignarTipo_ResponsableDesactivado_NoLoCambia()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador().Nombre, DniResponsable);
        servicio.Desactivar(DniResponsable, DniNuevo);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.AsignarTipo(DniValido, TipoUsuario.Docente().Nombre, DniResponsable));
        Assert.AreEqual(TipoUsuario.SinPermisos().Nombre, repositorio.ObtenerUsuarioPorDni(DniValido)!.Tipo.Nombre);
    }

    [TestMethod]
    public void Desactivar_ResponsableDesactivado_NoLoDesactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Administrador().Nombre, DniResponsable);
        servicio.Desactivar(DniResponsable, DniNuevo);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniValido, DniResponsable));
        Assert.AreEqual(EstadoUsuario.Activo, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Reactivar_ResponsableNoAdministrador_NoLoReactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);
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

    [TestMethod]
    public void ListarUsuarios_ResponsableNoAdministrador_LanzaExcepcion()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out _);

        Assert.ThrowsException<ServicioException>(() => servicio.ListarUsuarios(DniValido, null, 1));
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.Desactivar(DniValido, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.ModificarDatos(CrearDatos(dni: DniValido,
            nombre: "Pedro", apellido: "Suarez", correo: "pedro@hotmail.com"), DniValido));
        Assert.AreEqual("Felipe", repositorio.ObtenerUsuarioPorDni(DniValido)!.Nombre);
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);

        servicio.ModificarDatos(CrearDatos(dni: DniValido, nombre: "  Pedro ", apellido: " Suarez  ",
            correo: " pedro@hotmail.com "), DniValido);

        Usuario usuario = repositorio.ObtenerUsuarioPorDni(DniValido)!;
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
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.Desactivar(DniValido, DniNuevo));
        Assert.AreEqual(EstadoUsuario.Activo, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void Reactivar_ResponsableInexistente_NoLoReactiva()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.Desactivar(DniValido, DniResponsable);

        Assert.ThrowsException<ServicioException>(() => servicio.Reactivar(DniValido, DniNuevo));
        Assert.AreEqual(EstadoUsuario.Desactivado, repositorio.ObtenerUsuarioPorDni(DniValido)!.Estado);
    }

    [TestMethod]
    public void CrearUsuario_ResponsableDesactivado_NoLoGuarda()
    {
        ServicioAdministracionUsuarios servicio = CrearServicioConAdministrador(out RepositorioUsuario repositorio);
        servicio.AsignarTipo(DniValido, TipoUsuario.Administrador().Nombre, DniResponsable);
        servicio.Desactivar(DniResponsable, DniValido);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.CrearUsuario(CrearDtoNuevo(), TipoUsuario.Estudiante().Nombre, DniResponsable));
        Assert.IsNull(repositorio.ObtenerUsuarioPorCorreo(CorreoNuevo));
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

    private static void AgregarUsuarios(RepositorioUsuario repositorio, int cantidad)
    {
        for (int numero = 0; numero < cantidad; numero++)
        {
            repositorio.AgregarUsuario(new Usuario(CedulaValida(3000000 + numero), "Relleno",
                $"Apellido{numero:D3}", $"relleno{numero}@ucd.edu.uy", "Relleno2026"));
        }
    }

    // Arma una cedula con su digito verificador (datos de prueba: 7 digitos + verificador).
    private static string CedulaValida(int sinVerificador)
    {
        int[] pesos = { 2, 9, 8, 7, 6, 3, 4 };
        string digitos = sinVerificador.ToString("D7");
        int suma = digitos.Select((digito, i) => (digito - '0') * pesos[i]).Sum();
        return digitos + (10 - suma % 10) % 10;
    }

    private static ServicioAdministracionUsuarios CrearServicioConAdministrador(out RepositorioUsuario repositorio)
    {
        return CrearServicioConAdministrador(out repositorio, out _);
    }

    private static ServicioAdministracionUsuarios CrearServicioConAdministrador(out RepositorioUsuario repositorio,
        out RepositorioTipoUsuario repositorioTipos)
    {
        Memoria memoria = new Memoria();
        repositorio = new RepositorioUsuario(memoria);
        repositorioTipos = TiposDePrueba.Cargados(memoria);

        ServicioUsuario servicioUsuario = new ServicioUsuario(repositorio, repositorioTipos);
        servicioUsuario.Registrar(new UsuarioDto
        {
            Dni = DniValido, Nombre = "Felipe", Apellido = "Sosa", Correo = "felipe@hotmail.com",
            Contrasena = "VivaLaOrt2026"
        });
        Usuario administrador = new Usuario(dni: DniResponsable, nombre: "Root", apellido: "Admin",
            correo: "root@ucd.edu.uy", contrasena: "Root2026");
        administrador.AsignarTipo(TipoUsuario.Administrador());
        repositorio.AgregarUsuario(administrador);
        return new ServicioAdministracionUsuarios(repositorio, servicioUsuario, repositorioTipos);
    }
}
