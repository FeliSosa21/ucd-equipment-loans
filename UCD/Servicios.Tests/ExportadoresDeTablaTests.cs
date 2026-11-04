using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ExportadoresDeTablaTests
{
    [TestMethod]
    public void Exportar_ConCualquierExportadorDeLaInterfaz_DevuelveUnArchivoConSuFormatoExtensionYTipo()
    {
        List<IExportadorDeTabla> exportadores = new List<IExportadorDeTabla>
        {
            new ExportadorCsv(),
            new ExportadorXlsx()
        };
        TablaExportableDto tabla = CrearTabla();
        List<string> formatos = new List<string>();

        foreach (IExportadorDeTabla exportador in exportadores)
        {
            byte[] archivo = exportador.Exportar(tabla);

            Assert.IsTrue(archivo.Length > 0);
            Assert.IsTrue(exportador.Extension.StartsWith('.'));
            Assert.IsFalse(string.IsNullOrEmpty(exportador.TipoContenido));
            formatos.Add(exportador.Formato);
        }

        Assert.AreEqual(exportadores.Count, formatos.Distinct().Count());
    }

    private static TablaExportableDto CrearTabla()
    {
        TablaExportableDto tabla = new TablaExportableDto();
        tabla.Encabezados = new List<string> { "Nombre", "Edad" };
        tabla.Filas = new List<List<string>> { new List<string> { "Ana", "20" } };
        return tabla;
    }
}
