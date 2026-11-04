using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioTipoUsuarioSqlTests
{
    private const string ImagenPng = "data:image/png;base64,iVBORw0KGgo=";

    private DbContextOptions<SqlContext> _opciones = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _opciones = new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_TipoAgregado_OtroContextoLeeSuPresentacion()
    {
        GuardarConOtroContexto(CrearDocente());

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        TipoUsuario? leido = new RepositorioTipoUsuarioSql(contextoQueLee).ObtenerTipoPorNombre("Docente");

        Assert.IsNotNull(leido);
        Assert.AreEqual("Docentes de la facultad", leido.Descripcion);
        Assert.AreEqual(ImagenPng, leido.Imagen);
    }

    [TestMethod]
    public void ObtenerTipoPorNombre_TipoAgregado_OtroContextoLeeSusTopes()
    {
        GuardarConOtroContexto(CrearDocente());

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        TipoUsuario? leido = new RepositorioTipoUsuarioSql(contextoQueLee).ObtenerTipoPorNombre("Docente");

        Assert.AreEqual(40, leido?.MaxUnidadesPorCatalogo);
        Assert.AreEqual(3, leido?.MaxPrestamosConcurrentes);
    }

    private void GuardarConOtroContexto(TipoUsuario tipo)
    {
        using SqlContext contextoQueGuarda = new SqlContext(_opciones);
        new RepositorioTipoUsuarioSql(contextoQueGuarda).AgregarTipo(tipo);
    }

    private static TipoUsuario CrearDocente()
    {
        return new TipoUsuario(new PresentacionTipoUsuario("Docente", "Docentes de la facultad", ImagenPng),
            new TopesDePrestamo(maxUnidadesPorCatalogo: 40, maxPrestamosConcurrentes: 3),
            new[] { Permiso.ConsultarCatalogo, Permiso.SolicitarPrestamo });
    }
}
