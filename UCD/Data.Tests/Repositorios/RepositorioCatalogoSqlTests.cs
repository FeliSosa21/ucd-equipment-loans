using Dominio;
using Microsoft.EntityFrameworkCore;
using Data.Repositorios;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioCatalogoSqlTests
{
    private const string NombreCatalogo = "Aparatos Electronicos";

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
    public void AgregarCatalogo_CatalogoNull_LanzarArgumentNullException()
    {
        using SqlContext contexto = new SqlContext(_opciones);
        RepositorioCatalogoSql repositorio = new RepositorioCatalogoSql(contexto);

        Assert.ThrowsException<ArgumentNullException>(() => repositorio.AgregarCatalogo(null!));
    }
}