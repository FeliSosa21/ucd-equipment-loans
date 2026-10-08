using Data;
using Data.Repositorios;
using Dominio;

namespace Servicios.Tests;

[TestClass]
public class DatosDeEjemploTests
{
    private Memoria _memoria = null!;
    private RepositorioUsuario _usuarios = null!;
    private RepositorioPrestamo _prestamos = null!;
    private RepositorioCatalogo _repositorioCatalogo = null!;
    private DatosDeEjemplo _datos = null!;

    private static readonly DateTime Hoy = new DateTime(2026, 10, 7, 9, 0, 0);

    [TestInitialize]
    public void Inicializar()
    {
        _memoria = new Memoria();
        _usuarios = new RepositorioUsuario(_memoria);
        _prestamos = new RepositorioPrestamo(_memoria);
        _repositorioCatalogo = new RepositorioCatalogo(_memoria);
        _repositorioCatalogo.Guardar(new Catalogo(CargaInicial.NombreCatalogo));
        _datos = new DatosDeEjemplo(_usuarios, _prestamos, _repositorioCatalogo, new RelojFijo(Hoy));
    }

    [TestMethod]
    public void Cargar_MemoriaVacia_GuardaUsuariosConSuTipo()
    {
        _datos.Cargar();

        Assert.AreEqual(TipoUsuario.Docente, _usuarios.ObtenerUsuarioPorCorreo(DatosDeEjemplo.CorreoDocente)!.Tipo);
        Assert.AreEqual(TipoUsuario.Estudiante,
            _usuarios.ObtenerUsuarioPorCorreo(DatosDeEjemplo.CorreoEstudiante)!.Tipo);
        Assert.AreEqual(TipoUsuario.FuncionarioCE,
            _usuarios.ObtenerUsuarioPorCorreo(DatosDeEjemplo.CorreoFuncionario)!.Tipo);
    }

    [TestMethod]
    public void Cargar_MemoriaVacia_GuardaUnSupervisor()
    {
        _datos.Cargar();

        Assert.AreEqual(TipoUsuario.Supervisor,
            _usuarios.ObtenerUsuarioPorCorreo(DatosDeEjemplo.CorreoSupervisor)!.Tipo);
    }

    [TestMethod]
    public void Cargar_ConCatalogo_IncluyeUnTipoQueRequiereAprobacionConUnidadesDisponibles()
    {
        _datos.Cargar();

        TipoEquipamiento camara = _repositorioCatalogo.ObtenerTipoPorNombre(DatosDeEjemplo.TipoConAprobacion)!;
        Assert.IsTrue(camara.RequiereAprobacion);
        Assert.AreEqual(2, camara.CantidadDisponibles());
    }

    [TestMethod]
    public void Cargar_MemoriaVacia_GuardaPrestamosActivosConSusUnidadesPrestadas()
    {
        _datos.Cargar();

        IReadOnlyList<Prestamo> activos = _prestamos.ObtenerPrestamosActivos();
        Assert.AreEqual(2, activos.Count);
        Assert.IsTrue(activos.SelectMany(prestamo => prestamo.UnidadesAsignadas)
            .All(unidad => unidad.Estado == EstadoUnidadFisica.Prestada));
    }

    [TestMethod]
    public void Cargar_DosVeces_NoDuplica()
    {
        _datos.Cargar();
        _datos.Cargar();

        Assert.AreEqual(4, _memoria.Usuarios.Count);
        Assert.AreEqual(2, _prestamos.ObtenerPrestamosActivos().Count);
        Assert.AreEqual(6, _repositorioCatalogo.Obtener()!.Tipos.Count);
    }

    [TestMethod]
    public void Cargar_ConCatalogo_AgregaLosTiposDeEjemploAlCatalogo()
    {
        _datos.Cargar();

        Catalogo catalogo = _repositorioCatalogo.Obtener()!;
        Assert.AreEqual(6, catalogo.Tipos.Count);
        Assert.IsNotNull(catalogo.BuscarTipoPorNombre("Notebook"));
    }

    [TestMethod]
    public void Cargar_ConCatalogo_DejaElOsciloscopioDiscontinuado()
    {
        _datos.Cargar();

        Assert.IsFalse(_repositorioCatalogo.ObtenerTipoPorNombre("Osciloscopio")!.EstaActivo);
    }

    [TestMethod]
    public void Cargar_ConCatalogo_DejaUnaUnidadDelKitEnMantenimiento()
    {
        _datos.Cargar();

        TipoEquipamiento kit = _repositorioCatalogo.ObtenerTipoPorNombre("Kit Arduino")!;
        Assert.AreEqual(EstadoUnidadFisica.EnMantenimiento, kit.Unidades.Last().Estado);
    }

    [TestMethod]
    public void Cargar_MemoriaVacia_LosPrestamosTomanLaFechaDelReloj()
    {
        _datos.Cargar();

        Prestamo prestamoDelDocente = _prestamos.ObtenerPrestamosActivos()
            .Single(prestamo => prestamo.Solicitante.Correo == DatosDeEjemplo.CorreoDocente);
        Assert.AreEqual(Hoy.AddDays(-2), prestamoDelDocente.FechaHoraPrestamo);
    }

    [TestMethod]
    public void Cargar_ConCatalogo_LasUnidadesPrestadasNoFiguranDisponibles()
    {
        _datos.Cargar();

        TipoEquipamiento notebook = _repositorioCatalogo.ObtenerTipoPorNombre("Notebook")!;
        Assert.AreEqual(1, notebook.CantidadDisponibles());
    }

    [TestMethod]
    public void Cargar_SinCatalogo_LanzaInvalidOperationException()
    {
        Memoria memoria = new Memoria();
        DatosDeEjemplo datos = new DatosDeEjemplo(new RepositorioUsuario(memoria),
            new RepositorioPrestamo(memoria), new RepositorioCatalogo(memoria), new RelojFijo(Hoy));

        Assert.ThrowsException<InvalidOperationException>(() => datos.Cargar());
    }

    [TestMethod]
    public void Cargar_ConCatalogo_LasUnidadesTienenNumeroDeSerieArmadoDesdeSuId()
    {
        _datos.Cargar();

        TipoEquipamiento notebook = _repositorioCatalogo.ObtenerTipoPorNombre("Notebook")!;
        Assert.AreEqual("SNNTB0001", notebook.Unidades.First().NumSerie);
    }

    [TestMethod]
    public void Cargar_ConCatalogo_LosAccesoriosNoTienenNumeroDeSerie()
    {
        _datos.Cargar();

        TipoEquipamiento cableHdmi = _repositorioCatalogo.ObtenerTipoPorNombre("Cable HDMI")!;
        Assert.IsTrue(cableHdmi.Unidades.All(unidad => unidad.NumSerie is null));
    }
}
