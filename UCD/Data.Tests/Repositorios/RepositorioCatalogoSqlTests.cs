using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioCatalogoSqlTests
{
    private const string NombreCatalogo = "Aparatos Electronicos";
    private const string NombreSegundoCatalogo = "Camaras de foto";
    private const int IdInexistente = 999;
    private const string NombreTipo = "Laptop";
    private const string DescripcionTipo = "Laptop lenovo";
    private const int DuracionMaxPrestamo = 7;
    private const string IdUnidad = "EQP-0001";
    private const string UbicacionUnidad = "Laboratorio";

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    }

    [TestMethod]
    public void ObtenerCatalogosActivos_BaseVacia_DevuelveListaVacia()
    {
        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueLee);
        IReadOnlyList<Catalogo> resultado = repositorio.ObtenerCatalogosActivos();

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void AgregarCatalogo_CatalogoValido_OtroContextoLoEncuentra()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);

        using (SqlContext contextoQueGuarda = new SqlContext(_opciones))
        {
            RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueGuarda);
            repositorio.AgregarCatalogo(catalogo);
        }

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Catalogo? leido = contextoQueLee.Catalogos.Find(catalogo.Id);

        Assert.IsNotNull(leido);
        Assert.AreEqual(NombreCatalogo, leido.Nombre);
    }

    [TestMethod]
    public void AgregarCatalogo_CatalogoNull_LanzaArgumentNullException()
    {
        using SqlContext contexto = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contexto);

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.AgregarCatalogo(null!));
    }

    [TestMethod]
    public void ObtenerCatalogoActivoPorId_IdExistente_DevuelveElCatalogo()
    {
        Catalogo primero = new Catalogo(NombreCatalogo);
        Catalogo segundo = new Catalogo(NombreSegundoCatalogo);
        GuardarConOtroContexto(primero, segundo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        Catalogo? resultado = repositorioQueLee.ObtenerCatalogoActivoPorId(segundo.Id);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(segundo.Id, resultado.Id);
        Assert.AreEqual(NombreSegundoCatalogo, resultado.Nombre);
    }

    [TestMethod]
    public void ObtenerCatalogoActivoPorId_IdInexistente_DevuelveNull()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        GuardarConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        Catalogo? resultado = repositorioQueLee.ObtenerCatalogoActivoPorId(IdInexistente);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerCatalogoActivoPorId_CatalogoEliminado_DevuelveNull()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        GuardarConOtroContexto(catalogo);
        DarDeBajaConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        Catalogo? resultado = repositorioQueLee.ObtenerCatalogoActivoPorId(catalogo.Id);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerCatalogoActivoPorId_CatalogoConTiposYUnidades_LosTraeCargados()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        TipoEquipamiento tipo = new TipoEquipamiento(NombreTipo,DescripcionTipo,DuracionMaxPrestamo,requiereAprobacion: false, Categoria.Computacion);
        UnidadFisica unidad = new UnidadFisica(IdUnidad, UbicacionUnidad, null);
        tipo.AgregarUnidad(unidad);
        catalogo.AgregarTipo(tipo);
        GuardarConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        Catalogo? resultado = repositorioQueLee.ObtenerCatalogoActivoPorId(catalogo.Id);

        Assert.IsNotNull(resultado);
        TipoEquipamiento tipoLeido = resultado.Tipos.Single();
        Assert.AreEqual(NombreTipo, tipoLeido.Nombre);
        Assert.AreEqual(IdUnidad, tipoLeido.Unidades.Single().IdInstitucional);
    }

    [TestMethod]
    public void ObtenerCatalogosActivos_CatalogoEliminado_NoLoDevuelve()
    {
        Catalogo primerCatalogo = new Catalogo(NombreCatalogo);
        Catalogo segundoCatalogo = new Catalogo(NombreSegundoCatalogo);
        GuardarConOtroContexto(primerCatalogo, segundoCatalogo);
        DarDeBajaConOtroContexto(primerCatalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        IReadOnlyList<Catalogo> resultado = repositorioQueLee.ObtenerCatalogosActivos();

        Catalogo encontrado = resultado.Single();
        Assert.AreEqual(NombreSegundoCatalogo, encontrado.Nombre);
    }

    [TestMethod]
    public void ObtenerCatalogosActivos_VariosCatalogos_DevuelveOrdenadosPorNombre()
    {
        Catalogo primerCatalogo = new Catalogo(NombreCatalogo);
        Catalogo segundoCatalogo = new Catalogo(NombreSegundoCatalogo);
        GuardarConOtroContexto(segundoCatalogo, primerCatalogo);
        List<string> nombresEsperados = new List<string> { NombreCatalogo, NombreSegundoCatalogo };

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        IReadOnlyList<Catalogo> resultado = repositorioQueLee.ObtenerCatalogosActivos();

        List<string> nombresObtenidos = resultado.Select(catalogo => catalogo.Nombre).ToList();
        CollectionAssert.AreEqual(nombresEsperados, nombresObtenidos);
    }

    [TestMethod]
    public void ObtenerCatalogosActivos_CatalogoConTipo_NoLosCarga()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        TipoEquipamiento tipo = new TipoEquipamiento(NombreTipo,DescripcionTipo,DuracionMaxPrestamo,requiereAprobacion: false, Categoria.Computacion);
        UnidadFisica unidad = new UnidadFisica(IdUnidad, UbicacionUnidad, null);
        tipo.AgregarUnidad(unidad);
        catalogo.AgregarTipo(tipo);
        GuardarConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        IReadOnlyList<Catalogo> resultado = repositorioQueLee.ObtenerCatalogosActivos();

        Catalogo encontrado = resultado.Single();
        Assert.AreEqual(0, encontrado.Tipos.Count);
    }

    [TestMethod]
    [DataRow(NombreCatalogo)]
    [DataRow("APARATOS ELECTRONICOS")]
    [DataRow(" Aparatos Electronicos ")]
    public void ExisteActivoConNombre_NombreEquivalente_DevuelveTrue(string nombreBuscado)
    {
        GuardarConOtroContexto(new Catalogo(NombreCatalogo));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        bool existe = repositorioQueLee.ExisteActivoConNombre(nombreBuscado, idExcluido: null);

        Assert.IsTrue(existe);
    }

    [TestMethod]
    public void ExisteActivoConNombre_MismoCatalogo_DevuelveFalse()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        GuardarConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        bool existe = repositorioQueLee.ExisteActivoConNombre(NombreCatalogo, idExcluido: catalogo.Id);

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void ExisteActivoConNombre_CatalogoEliminadoConEseNombre_DevuelveFalse()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        GuardarConOtroContexto(catalogo);
        DarDeBajaConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        bool existe = repositorioQueLee.ExisteActivoConNombre(NombreCatalogo, idExcluido: null);

        Assert.IsFalse(existe);
    }

    private void GuardarConOtroContexto(params Catalogo[] catalogos)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueGuarda);

        foreach (Catalogo catalogo in catalogos)
        {
            repositorio.AgregarCatalogo(catalogo);
        }
    }

    private void DarDeBajaConOtroContexto(Catalogo catalogo)
    {
        using SqlContext contextoQueModifica = new SqlContext(_opciones);
        Catalogo guardado = contextoQueModifica.Catalogos.Find(catalogo.Id)!;
        guardado.DarDeBaja();
        contextoQueModifica.SaveChanges();
    }
}