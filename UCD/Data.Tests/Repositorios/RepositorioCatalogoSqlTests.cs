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
    private const string IdSegundaUnidad = "EQP-0002";
    private const string NombreSegundoTipo = "Proyector";

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
        Catalogo catalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreCatalogo);
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
        Catalogo catalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreCatalogo);
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

    [TestMethod]
    public void ExisteActivoConNombre_NombreLibre_DevuelveFalse()
    {
        GuardarConOtroContexto(new Catalogo(NombreCatalogo));

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        bool existe = repositorioQueLee.ExisteActivoConNombre(NombreSegundoCatalogo, idExcluido: null);

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void ExisteActivoConNombre_NombreNull_LanzaArgumentNullException()
    {
        using SqlContext contexto = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contexto);

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.ExisteActivoConNombre(null!, idExcluido: null));
    }

    [TestMethod]
    public void ExisteUnidadConIdInstitucional_UnidadEnOtroCatalogo_DevuelveTrue()
    {
        Catalogo primerCatalogo = new Catalogo(NombreCatalogo);
        Catalogo segundoCatalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreSegundoCatalogo);
        GuardarConOtroContexto(primerCatalogo, segundoCatalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        bool existe = repositorioQueLee.ExisteUnidadConIdInstitucional(IdUnidad);

        Assert.IsTrue(existe);
    }

    [TestMethod]
    public void ExisteUnidadConIdInstitucional_UnidadEnCatalogoEliminado_DevuelveTrue()
    {
        Catalogo catalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreCatalogo);
        GuardarConOtroContexto(catalogo);
        DarDeBajaConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        bool existe = repositorioQueLee.ExisteUnidadConIdInstitucional(IdUnidad);

        Assert.IsTrue(existe);
    }

    [TestMethod]
    public void ExisteUnidadConIdInstitucional_IdLibre_DevuelveFalse()
    {
        Catalogo catalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreCatalogo);
        GuardarConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        bool existe = repositorioQueLee.ExisteUnidadConIdInstitucional(IdSegundaUnidad);

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void ActualizarCatalogo_NombreModificado_OtroContextoVeElCambio()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        GuardarConOtroContexto(catalogo);

        using (SqlContext contextoQueModifica = new SqlContext(_opciones))
        {
            RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueModifica);
            Catalogo obtenido = repositorio.ObtenerCatalogoActivoPorId(catalogo.Id)!;
            obtenido.ModificarNombre(NombreSegundoCatalogo);
            repositorio.ActualizarCatalogo(obtenido);
        }

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        Catalogo leido = contextoQueLee.Catalogos.Find(catalogo.Id)!;

        Assert.AreEqual(NombreSegundoCatalogo, leido.Nombre);
    }

    [TestMethod]
    public void ActualizarCatalogo_TipoAgregado_OtroContextoLoEncuentra()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        GuardarConOtroContexto(catalogo);

        using (SqlContext contextoQueModifica = new SqlContext(_opciones))
        {
            RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueModifica);
            Catalogo obtenido = repositorio.ObtenerCatalogoActivoPorId(catalogo.Id)!;
            TipoEquipamiento tipo = CrearTipoEquipamientoConUnidad(NombreTipo, IdUnidad);
            obtenido.AgregarTipo(tipo);
            repositorio.ActualizarCatalogo(obtenido);
        }

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        Catalogo leido = repositorioQueLee.ObtenerCatalogoActivoPorId(catalogo.Id)!;

        Assert.AreEqual(NombreTipo, leido.Tipos.Single().Nombre);
    }

    [TestMethod]
    public void ActualizarCatalogo_CatalogoDadoDeBaja_YaNoApareceEntreLosActivos()
    {
        Catalogo catalogo = new Catalogo(NombreCatalogo);
        GuardarConOtroContexto(catalogo);

        DarDeBajaConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        IReadOnlyList<Catalogo> resultado = repositorioQueLee.ObtenerCatalogosActivos();

        Assert.AreEqual(0, resultado.Count);
    }

    [TestMethod]
    public void ActualizarCatalogo_CatalogoNull_LanzaArgumentNullException()
    {
        using SqlContext contexto = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contexto);

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.ActualizarCatalogo(null!));
    }

    [TestMethod]
    [DataRow(NombreSegundoTipo)]
    [DataRow("PROYECTOR")]
    [DataRow(" Proyector ")]
    public void ObtenerTipoPorNombre_TipoExistente_DevuelveElTipo(string nombreTipo)
    {
        Catalogo catalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreCatalogo);
        TipoEquipamiento tipo = CrearTipoEquipamientoConUnidad(NombreSegundoTipo, IdSegundaUnidad);
        catalogo.AgregarTipo(tipo);
        GuardarConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        TipoEquipamiento? resultado = repositorioQueLee.ObtenerTipoPorNombre(catalogo.Id, nombreTipo);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(NombreSegundoTipo, resultado.Nombre);
        Assert.AreEqual(IdSegundaUnidad, resultado.Unidades.Single().IdInstitucional);
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_TipoDeOtroCatalogo_DevuelveNull()
    {
        Catalogo primerCatalogo = new Catalogo(NombreCatalogo);
        Catalogo segundoCatalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreSegundoCatalogo);
        GuardarConOtroContexto(primerCatalogo, segundoCatalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        TipoEquipamiento? resultado = repositorioQueLee.ObtenerTipoPorNombre(primerCatalogo.Id, NombreTipo);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_CatalogoEliminado_DevuelveNull()
    {
        Catalogo catalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreCatalogo);
        GuardarConOtroContexto(catalogo);
        DarDeBajaConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        TipoEquipamiento? resultado = repositorioQueLee.ObtenerTipoPorNombre(catalogo.Id, NombreTipo);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_NombreNull_LanzaArgumentNullException()
    {
        using SqlContext contexto = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contexto);

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.ObtenerTipoPorNombre(IdInexistente, null!));
    }

    [TestMethod]
    public void ObtenerCatalogosParaExportar_ActivosYEliminados_DevuelveTodos()
    {
        Catalogo primerCatalogo = new Catalogo(NombreCatalogo);
        Catalogo segundoCatalogo = new Catalogo(NombreSegundoCatalogo);
        GuardarConOtroContexto(primerCatalogo, segundoCatalogo);
        DarDeBajaConOtroContexto(primerCatalogo);
        List<string> nombresEsperados = new List<string> { NombreCatalogo, NombreSegundoCatalogo };

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueLee);
        IReadOnlyList<Catalogo> resultado = repositorio.ObtenerCatalogosParaExportar();

        List<string> nombresObtenidos = resultado.Select(catalogo => catalogo.Nombre).ToList();
        CollectionAssert.AreEqual(nombresEsperados, nombresObtenidos);
    }

    [TestMethod]
    public void ObtenerCatalogosParaExportar_CatalogoConTipos_TraeLosTiposSinUnidades()
    {
        Catalogo catalogo = CrearCatalogoConTipoEquipamientoYUnidad(NombreCatalogo);
        GuardarConOtroContexto(catalogo);

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueLee);
        IReadOnlyList<Catalogo> resultado = repositorio.ObtenerCatalogosParaExportar();

        TipoEquipamiento tipo = resultado.Single().Tipos.Single();
        Assert.AreEqual(NombreTipo, tipo.Nombre);
        Assert.AreEqual(0, tipo.Unidades.Count);
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

    private static TipoEquipamiento CrearTipoEquipamientoConUnidad(string nombreTipo, string idUnidad)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombreTipo, DescripcionTipo, DuracionMaxPrestamo,
            requiereAprobacion: false, Categoria.Computacion);
        tipo.AgregarUnidad(new UnidadFisica(idUnidad, UbicacionUnidad, null));

        return tipo;
    }

    private static Catalogo CrearCatalogoConTipoEquipamientoYUnidad(string nombreCatalogo)
    {
        Catalogo catalogo = new Catalogo(nombreCatalogo);
        TipoEquipamiento tipo = CrearTipoEquipamientoConUnidad(NombreTipo, IdUnidad);
        catalogo.AgregarTipo(tipo);

        return catalogo;
    }

    private void DarDeBajaConOtroContexto(Catalogo catalogo)
    {
        using SqlContext contextoQueModifica = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueModifica);
        Catalogo obtenido = repositorio.ObtenerCatalogoActivoPorId(catalogo.Id)!;
        obtenido.DarDeBaja();
        repositorio.ActualizarCatalogo(obtenido);
    }
}