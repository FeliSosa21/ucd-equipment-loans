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

    [TestMethod]
    public void ObtenerTipoPorNombre_TipoAgregado_OtroContextoLeeSusPermisos()
    {
        GuardarConOtroContexto(CrearDocente());

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        TipoUsuario? leido = new RepositorioTipoUsuarioSql(contextoQueLee).ObtenerTipoPorNombre("Docente");

        Assert.IsTrue(leido!.TienePermiso(Permiso.SolicitarPrestamo));
        Assert.IsFalse(leido.TienePermiso(Permiso.GestionarUsuarios));
    }

    [TestMethod]
    public void ObtenerTodos_DosTiposAgregados_LosDevuelveEnElOrdenEnQueSeAgregaron()
    {
        GuardarConOtroContexto(CrearDocente());
        GuardarConOtroContexto(CrearEstudiante());

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<TipoUsuario> leidos = new RepositorioTipoUsuarioSql(contextoQueLee).ObtenerTodos();

        CollectionAssert.AreEqual(new List<string> { "Docente", "Estudiante" },
            leidos.Select(tipo => tipo.Nombre).ToList());
    }

    [TestMethod]
    public void ActualizarTipo_TipoModificado_OtroContextoLeeLosCambios()
    {
        using (SqlContext contextoQueModifica = new SqlContext(_opciones))
        {
            RepositorioTipoUsuarioSql repositorio = new RepositorioTipoUsuarioSql(contextoQueModifica);
            TipoUsuario docente = CrearDocente();
            repositorio.AgregarTipo(docente);

            docente.Modificar("Docentes y ayudantes", ImagenPng, new TopesDePrestamo(10, 2));
            repositorio.ActualizarTipo(docente);
        }

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        TipoUsuario? leido = new RepositorioTipoUsuarioSql(contextoQueLee).ObtenerTipoPorNombre("Docente");

        Assert.AreEqual("Docentes y ayudantes", leido?.Descripcion);
        Assert.AreEqual(10, leido?.MaxUnidadesPorCatalogo);
    }

    [TestMethod]
    public void ObtenerTodos_SeisTiposAgregados_DevuelveLosTiposPreparados()
    {
        foreach (TipoUsuario tipo in TipoUsuario.Todos())
        {
            GuardarConOtroContexto(tipo);
        }

        using SqlContext contexto = new SqlContext(_opciones);
        IReadOnlyList<TipoUsuario> tipos = new RepositorioTipoUsuarioSql(contexto).ObtenerTodos();

        CollectionAssert.AreEqual(TipoUsuario.Todos().Select(tipo => tipo.Nombre).ToList(),
            tipos.Select(tipo => tipo.Nombre).ToList());
    }

    [TestMethod]
    public void EnsureCreated_BaseNueva_NoInsertaTiposDeUsuario()
    {
        using (SqlContext contextoQueCrea = new SqlContext(_opciones))
        {
            contextoQueCrea.Database.EnsureCreated();
        }

        using SqlContext contextoQueLee = new SqlContext(_opciones);
        IReadOnlyList<TipoUsuario> tipos = new RepositorioTipoUsuarioSql(contextoQueLee).ObtenerTodos();

        Assert.AreEqual(0, tipos.Count);
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

    private static TipoUsuario CrearEstudiante()
    {
        return new TipoUsuario(new PresentacionTipoUsuario("Estudiante", "Estudiantes de la facultad", ImagenPng),
            new TopesDePrestamo(maxUnidadesPorCatalogo: 2, maxPrestamosConcurrentes: 2),
            new[] { Permiso.ConsultarCatalogo, Permiso.SolicitarPrestamo });
    }
}
