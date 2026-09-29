using Data;
using Data.Repositorios;
using Dominio;

namespace Servicios.Tests;

[TestClass]
public class CargaInicialTests
{
    [TestMethod]
    public void CargarRoot_MemoriaVacia_GuardaAdministradorQueDebeCambiarContrasena()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        CargaInicial carga = new CargaInicial(repositorio);

        carga.CargarRoot();

        Usuario? root = repositorio.ObtenerUsuarioPorCorreo(CargaInicial.CorreoRoot);
        Assert.IsNotNull(root);
        Assert.AreEqual(TipoUsuario.Administrador, root.Tipo);
        Assert.IsTrue(root.DebeCambiarContrasena);
    }

    [TestMethod]
    public void CargarRoot_DosVeces_NoLoDuplica()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        CargaInicial carga = new CargaInicial(repositorio);

        carga.CargarRoot();
        carga.CargarRoot();

        Assert.AreEqual(1, memoria.Usuarios.Count);
    }
}
