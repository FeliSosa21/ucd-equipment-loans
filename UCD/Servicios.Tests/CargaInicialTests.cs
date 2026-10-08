using Data;
using Data.Repositorios;
using Dominio;

namespace Servicios.Tests;

[TestClass]
public class CargaInicialTests
{
    [TestMethod]
    public void CargarRoot_MemoriaVacia_GuardaAdministradorQueNoDebeCambiarContrasena()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        CargaInicial carga = CrearCarga(memoria);

        carga.CargarRoot();

        Usuario? root = repositorio.ObtenerUsuarioPorCorreo(CargaInicial.CorreoRoot);
        Assert.IsNotNull(root);
        Assert.AreEqual(TipoUsuario.Administrador, root.Tipo);
        Assert.IsFalse(root.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CargarRoot_DosVeces_NoLoDuplica()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        CargaInicial carga = CrearCarga(memoria);

        carga.CargarRoot();
        carga.CargarRoot();

        Assert.AreEqual(1, repositorio.ObtenerTodos().Count);
    }

    [TestMethod]
    public void CargarCatalogo_MemoriaVacia_GuardaUnCatalogo()
    {
        Memoria memoria = new Memoria();
        RepositorioCatalogo catalogos = new RepositorioCatalogo(memoria);
        CargaInicial carga = CrearCarga(memoria);

        carga.CargarCatalogo();

        Catalogo? catalogo = catalogos.Obtener();
        Assert.IsNotNull(catalogo);
        Assert.AreEqual(CargaInicial.NombreCatalogo, catalogo.Nombre);
        Assert.AreEqual(0, catalogo.Tipos.Count);
    }

    [TestMethod]
    public void CargarCatalogo_DosVeces_ConservaElPrimerCatalogo()
    {
        Memoria memoria = new Memoria();
        RepositorioCatalogo catalogos = new RepositorioCatalogo(memoria);
        CargaInicial carga = CrearCarga(memoria);
        carga.CargarCatalogo();
        Catalogo? primero = catalogos.Obtener();

        carga.CargarCatalogo();

        Assert.AreSame(primero, catalogos.Obtener());
    }

    private static CargaInicial CrearCarga(Memoria memoria)
    {
        return new CargaInicial(new RepositorioUsuario(memoria), new RepositorioCatalogo(memoria));
    }
}
