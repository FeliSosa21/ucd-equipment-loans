using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioUnidadesTests
{
    private const string DniFuncionario = "12345672";
    private const string DniDocente = "45678905";

    [TestMethod]
    public void Listar_SinFiltros_DevuelveLasUnidadesDeTodosLosTipos()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(new UnidadFisica("PRY-0001", "Deposito A", "SN123"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        UnidadFisica conObservaciones = new UnidadFisica("NTB-0001", "Laboratorio 2", null);
        conObservaciones.ModificarInfo("Laboratorio 2", null, "Pantalla rayada");
        notebook.AgregarUnidad(conObservaciones);
        ServicioUnidades servicio = CrearServicio(proyector, notebook);

        List<UnidadDetalleDto> unidades = servicio.Listar(null, null);

        Assert.AreEqual(2, unidades.Count);
        Assert.AreEqual("PRY-0001", unidades[0].IdInstitucional);
        Assert.AreEqual("Proyector Epson", unidades[0].Tipo);
        Assert.AreEqual("Deposito A", unidades[0].UbicacionFisica);
        Assert.AreEqual("SN123", unidades[0].NumeroSerie);
        Assert.AreEqual("Disponible", unidades[0].Estado);
        Assert.AreEqual("Pantalla rayada", unidades[1].Observaciones);
    }

    [TestMethod]
    public void Listar_FiltroPorTipo_DevuelveSoloLasDeEseTipo()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad("PRY-0001"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        notebook.AgregarUnidad(CrearUnidad("NTB-0001"));
        ServicioUnidades servicio = CrearServicio(proyector, notebook);

        List<UnidadDetalleDto> unidades = servicio.Listar("Notebook Lenovo", null);

        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("NTB-0001", unidades[0].IdInstitucional);
    }

    [TestMethod]
    public void Listar_FiltroPorEstado_DevuelveSoloLasDeEseEstado()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica enMantenimiento = CrearUnidad("PRY-0002");
        enMantenimiento.AMantenimiento();
        proyector.AgregarUnidad(CrearUnidad("PRY-0001"));
        proyector.AgregarUnidad(enMantenimiento);
        ServicioUnidades servicio = CrearServicio(proyector);

        List<UnidadDetalleDto> unidades = servicio.Listar(null, "EnMantenimiento");

        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("PRY-0002", unidades[0].IdInstitucional);
    }

    [TestMethod]
    public void Registrar_DatosValidos_LaAgregaDisponible()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.Registrar(CrearDatos("Proyector Epson", "PRY-0001", "SN123"), DniFuncionario);

        List<UnidadDetalleDto> unidades = servicio.Listar(null, null);
        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("PRY-0001", unidades[0].IdInstitucional);
        Assert.AreEqual("Proyector Epson", unidades[0].Tipo);
        Assert.AreEqual("Deposito A", unidades[0].UbicacionFisica);
        Assert.AreEqual("SN123", unidades[0].NumeroSerie);
        Assert.AreEqual("Disponible", unidades[0].Estado);
    }

    [TestMethod]
    public void Registrar_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(CrearDatos("Proyector Epson", "PRY-0001", null), DniDocente));
    }

    [TestMethod]
    public void Registrar_TipoInexistente_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio();
        Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(CrearDatos("Proyector Epson", "PRY-0001", null), DniFuncionario));
    }

    [TestMethod]
    public void Registrar_IdRepetidoEnOtroTipo_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad("EQP-0001"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        ServicioUnidades servicio = CrearServicio(proyector, notebook);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(CrearDatos("Notebook Lenovo", "EQP-0001", null), DniFuncionario));
    }

    [TestMethod]
    public void Registrar_SerieRepetida_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(new UnidadFisica("PRY-0001", "Deposito", "SN123"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        ServicioUnidades servicio = CrearServicio(proyector, notebook);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(CrearDatos("Notebook Lenovo", "NTB-0001", "sn123"), DniFuncionario));
    }

    [TestMethod]
    public void Registrar_DosUnidadesSinSerie_RegistraAmbas()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad("PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.Registrar(CrearDatos("Proyector Epson", "PRY-0002", null), DniFuncionario);

        Assert.AreEqual(2, servicio.Listar(null, null).Count);
    }

    [TestMethod]
    public void Registrar_TipoDiscontinuado_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.Discontinuar();
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(CrearDatos("Proyector Epson", "PRY-0001", null), DniFuncionario));
    }

    [DataTestMethod]
    [DataRow("pry-0001")]
    [DataRow("PRY0001")]
    [DataRow("PR-00001")]
    public void Registrar_IdConFormatoInvalido_LanzaServicioException(string idInstitucional)
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Registrar(CrearDatos("Proyector Epson", idInstitucional, null), DniFuncionario));
    }

    [TestMethod]
    public void Registrar_TextosConEspacios_LosGuardaRecortados()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = new DatosUnidadDto
        {
            NombreTipo = "Proyector Epson",
            IdInstitucional = " PRY-0001 ",
            UbicacionFisica = "  Deposito A  ",
            NumeroSerie = "   "
        };

        servicio.Registrar(datos, DniFuncionario);

        UnidadDetalleDto unidad = servicio.Listar(null, null)[0];
        Assert.AreEqual("PRY-0001", unidad.IdInstitucional);
        Assert.AreEqual("Deposito A", unidad.UbicacionFisica);
        Assert.IsNull(unidad.NumeroSerie);
    }

    [TestMethod]
    public void Modificar_DatosValidos_ActualizaLaUnion()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(new UnidadFisica("PRY-0001", "Deposito A", "SN123"));
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.Modificar(CrearEdicion("PRY-0001", "SN999"), DniFuncionario);

        UnidadDetalleDto unidad = servicio.Listar(null, null)[0];
        Assert.AreEqual("Laboratorio 3", unidad.UbicacionFisica);
        Assert.AreEqual("SN999", unidad.NumeroSerie);
        Assert.AreEqual("Lampara nueva", unidad.Observaciones);
    }

    [TestMethod]
    public void Modificar_ResponsableSinPermiso_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad("PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(CrearEdicion("PRY-0001", null), DniDocente));
    }

    [TestMethod]
    public void Modificar_UnidadInexistente_LanzaServicioException()
    {
        ServicioUnidades servicio = CrearServicio(CrearTipo("Proyector Epson"));

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(CrearEdicion("PRY-0001", null), DniFuncionario));
    }

    [TestMethod]
    public void Modificar_SerieDeOtraUnidad_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(new UnidadFisica("PRY-0001", "Deposito A", "SN123"));
        proyector.AgregarUnidad(new UnidadFisica("PRY-0002", "Deposito A", "SN456"));
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.Modificar(CrearEdicion("PRY-0002", "SN123"), DniFuncionario));
    }

    [TestMethod]
    public void Modificar_MismaSerie_ActualizaLosDemasDatos()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(new UnidadFisica("PRY-0001", "Deposito A", "SN123"));
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.Modificar(CrearEdicion("PRY-0001", "SN123"), DniFuncionario);

        Assert.AreEqual("Laboratorio 3", servicio.Listar(null, null)[0].UbicacionFisica);
    }

    [TestMethod]
    public void Modificar_UbicacionMuyCorta_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad("PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);
        DatosUnidadDto datos = CrearEdicion("PRY-0001", null);
        datos.UbicacionFisica = "AB";

        Assert.ThrowsException<ServicioException>(() => servicio.Modificar(datos, DniFuncionario));
    }

    [TestMethod]
    public void EnviarAMantenimiento_UnidadDisponible_QuedaEnMantenimiento()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad("PRY-0001"));
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.EnviarAMantenimiento("PRY-0001", DniFuncionario);

        Assert.AreEqual("EnMantenimiento", servicio.Listar(null, null)[0].Estado);
    }

    [TestMethod]
    public void EnviarAMantenimiento_UnidadDadaDeBaja_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad("PRY-0001");
        unidad.DarDeBaja();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.EnviarAMantenimiento("PRY-0001", DniFuncionario));
    }

    [TestMethod]
    public void VolverADisponible_UnidadEnMantenimiento_QuedaDisponible()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad("PRY-0001");
        unidad.AMantenimiento();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.VolverADisponible("PRY-0001", DniFuncionario);

        Assert.AreEqual("Disponible", servicio.Listar(null, null)[0].Estado);
    }

    [TestMethod]
    public void VolverADisponible_UnidadPrestada_LanzaServicioException()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica unidad = CrearUnidad("PRY-0001");
        unidad.Prestada();
        proyector.AgregarUnidad(unidad);
        ServicioUnidades servicio = CrearServicio(proyector);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.VolverADisponible("PRY-0001", DniFuncionario));
    }

    private static ServicioUnidades CrearServicio(params TipoEquipamiento[] tipos)
    {
        Memoria memoria = new Memoria();
        RepositorioCatalogo repositorioCatalogo = new RepositorioCatalogo(memoria);
        Catalogo catalogo = new Catalogo("Catalogo UCD");
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        repositorioCatalogo.Guardar(catalogo);

        RepositorioUsuario repositorioUsuario = new RepositorioUsuario(memoria);
        repositorioUsuario.AgregarUsuario(CrearUsuario(DniFuncionario, "funcionario@ucd.edu.uy",
            TipoUsuario.FuncionarioCE));
        repositorioUsuario.AgregarUsuario(CrearUsuario(DniDocente, "docente@ucd.edu.uy", TipoUsuario.Docente));
        return new ServicioUnidades(repositorioCatalogo, repositorioUsuario);
    }

    private static Usuario CrearUsuario(string dni, string correo, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni, "Nombre", "Apellido", correo, "VivaLaOrt2026");
        usuario.AsignarTipo(tipo);
        return usuario;
    }

    private static TipoEquipamiento CrearTipo(string nombre)
    {
        return new TipoEquipamiento(nombre, "Descripcion", 3, false, Categoria.Audiovisual);
    }

    private static UnidadFisica CrearUnidad(string idInstitucional)
    {
        return new UnidadFisica(idInstitucional, "Deposito A", null);
    }

    private static DatosUnidadDto CrearDatos(string nombreTipo, string idInstitucional, string? numeroSerie)
    {
        return new DatosUnidadDto
        {
            NombreTipo = nombreTipo,
            IdInstitucional = idInstitucional,
            UbicacionFisica = "Deposito A",
            NumeroSerie = numeroSerie
        };
    }

    private static DatosUnidadDto CrearEdicion(string idInstitucional, string? numeroSerie)
    {
        return new DatosUnidadDto
        {
            IdInstitucional = idInstitucional,
            UbicacionFisica = "Laboratorio 3",
            NumeroSerie = numeroSerie,
            Observaciones = "Lampara nueva"
        };
    }
}