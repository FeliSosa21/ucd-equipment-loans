using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioCatalogoTests
{
    [TestMethod]
    public void ConsultarTipos_FiltroVacio_DevuelveTodos()
    {
        var servicio = CrearServicio(CrearTipo(nombre:"Proyector Sony"), CrearTipo(nombre:"Proyector Microsoft"));

        var resultado = servicio.ConsultarTipos(new FiltroCatalogoDto());

        Assert.AreEqual(2, resultado.Count);
        Assert.AreEqual("Proyector Sony", resultado[0].Nombre);
        Assert.AreEqual("Proyector Microsoft", resultado[1].Nombre);
    }

    private static ServicioCatalogo CrearServicio(params TipoEquipamiento[] tipos)
    {
        var memoria = new Memoria();
        var repositorio = new RepositorioCatalogo(memoria);
        var catalogo = new Catalogo("Catalogo Central");
        foreach (var tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        repositorio.Guardar(catalogo);
        return new ServicioCatalogo(repositorio);
    }

    private static TipoEquipamiento CrearTipo(
        string nombre = "Proyector Epson EB-770Fi",
        string descripcion = "Proyector para aulas",
        int duracionMaxPrestamo = 8,
        bool requiereAprobacion = true,
        Categoria categoria = Categoria.Audiovisual)
    {
        return new TipoEquipamiento(nombre, descripcion, duracionMaxPrestamo, requiereAprobacion, categoria);
    }
}