using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioTiposEquipamientoTests
{
    [TestMethod]
    public void Listar_SinFiltros_DevuelveTodosConSusDatos()
    {
        TipoEquipamiento proyector = new TipoEquipamiento("Proyector Epson", "Para aulas", 3, true,
            Categoria.Audiovisual);
        TipoEquipamiento notebook = new TipoEquipamiento("Notebook Lenovo", "ThinkPad T14", 5, false,
            Categoria.Computacion);
        notebook.Discontinuar();
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar(null, null);

        Assert.AreEqual(2, tipos.Count);
        Assert.AreEqual("Proyector Epson", tipos[0].Nombre);
        Assert.AreEqual("Para aulas", tipos[0].Descripcion);
        Assert.AreEqual(3, tipos[0].DuracionMaxPrestamo);
        Assert.IsTrue(tipos[0].RequiereAprobacion);
        Assert.AreEqual("Audiovisual", tipos[0].Categoria);
        Assert.AreEqual("Activo", tipos[0].Estado);
        Assert.AreEqual("Discontinuado", tipos[1].Estado);
    }

    [TestMethod]
    public void Listar_SinCatalogo_LanzaServicioException()
    {
        Memoria memoria = new Memoria();
        ServicioTiposEquipamiento servicio = new ServicioTiposEquipamiento(new RepositorioCatalogo(memoria),
            new RepositorioUsuario(memoria));

        Assert.ThrowsException<ServicioException>(() => servicio.Listar(null, null));
    }

    [TestMethod]
    public void Listar_FiltroPorNombre_DevuelvelosQueContienenElTexto()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson", Categoria.Audiovisual);
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo", Categoria.Computacion);
        ServicioTiposEquipamiento servicio = CrearServicio(proyector, notebook);

        List<TipoEquipamientoDetalleDto> tipos = servicio.Listar("epson", null);

        Assert.AreEqual(1, tipos.Count);
        Assert.AreEqual("Proyector Epson", tipos[0].Nombre);
    }

    private static ServicioTiposEquipamiento CrearServicio(params TipoEquipamiento[] tipos)
    {
        Memoria memoria = new Memoria();
        RepositorioCatalogo repositorioCatalogo = new RepositorioCatalogo(memoria);
        Catalogo catalogo = new Catalogo("Catalogo UCD");
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        repositorioCatalogo.Guardar(catalogo);
        return new ServicioTiposEquipamiento(repositorioCatalogo, new RepositorioUsuario(memoria));
    }

    private static TipoEquipamiento CrearTipo(string nombre, Categoria categoria)
    {
        return new TipoEquipamiento(nombre, "Descripcion", 3, false, categoria);
    }
}