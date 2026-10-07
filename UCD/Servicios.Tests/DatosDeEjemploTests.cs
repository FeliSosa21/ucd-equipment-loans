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

    [TestInitialize]
    public void Inicializar()
    {
        _memoria = new Memoria();
        _usuarios = new RepositorioUsuario(_memoria);
        _prestamos = new RepositorioPrestamo(_memoria);
        _repositorioCatalogo = new RepositorioCatalogo(_memoria);
        _repositorioCatalogo.Guardar(new Catalogo(CargaInicial.NombreCatalogo));
        _datos = new DatosDeEjemplo(_usuarios, _prestamos, _repositorioCatalogo);
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

        Assert.AreEqual(3, _memoria.Usuarios.Count);
        Assert.AreEqual(2, _prestamos.ObtenerPrestamosActivos().Count);
        Assert.AreEqual(5, _repositorioCatalogo.Obtener()!.Tipos.Count);
    }

    [TestMethod]
    public void Cargar_ConCatalogo_AgregaLosTiposDeEjemploAlCatalogo()
    {
        _datos.Cargar();

        Catalogo catalogo = _repositorioCatalogo.Obtener()!;
        Assert.AreEqual(5, catalogo.Tipos.Count);
        Assert.IsNotNull(catalogo.BuscarTipoPorNombre("Notebook"));
        Assert.IsFalse(catalogo.BuscarTipoPorNombre("Osciloscopio")!.EstaActivo);
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
            new RepositorioPrestamo(memoria), new RepositorioCatalogo(memoria));

        Assert.ThrowsException<InvalidOperationException>(() => datos.Cargar());
    }

    [TestMethod]
    public void Cargar_ConCatalogo_LasUnidadesTienenNumeroDeSerieArmadoDesdeSuId()
    {
        _datos.Cargar();

        TipoEquipamiento notebook = _repositorioCatalogo.ObtenerTipoPorNombre("Notebook")!;
        Assert.AreEqual("SNNTB0001", notebook.Unidades.First().NumSerie);
    }
}
