using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioCatalogoSqlTests
{
    private const string NombreCatalogo = "Aparatos Electronicos";
    private const string NombreSegundoCatalogo = "Camaras de foto";

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

        using (SqlContext contextoQueGuarda = new SqlContext(_opciones))
        {
            RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contextoQueGuarda);
            repositorio.AgregarCatalogo(primero);
            repositorio.AgregarCatalogo(segundo);
        }

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorioQueLee = new RepositorioCatalogoSql(contextoQueLee);
        Catalogo? resultado = repositorioQueLee.ObtenerCatalogoPorId(segundo.Id);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(segundo.Id, resultado.Id);
        Assert.AreEqual(NombreSegundoCatalogo, resultado.Nombre);
    }
}