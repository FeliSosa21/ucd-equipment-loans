using System.Text;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ExportadorCsvTests
{
    private const char MarcaDeOrdenDeBytes = '\uFEFF';

    [TestMethod]
    public void Exportar_TablaSinFilas_EscribeSoloElEncabezado()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(encabezados: new List<string> { "Nombre", "Edad" });

        byte[] bytes = exportador.Exportar(tabla);

        Assert.AreEqual("Nombre,Edad\r\n", LeerTexto(bytes));
    }

    [TestMethod]
    public void Formato_ExportadorCsv_DevuelveCSV()
    {
        ExportadorCsv exportador = new ExportadorCsv();

        string formato = exportador.Formato;

        Assert.AreEqual("CSV", formato);
    }

    [TestMethod]
    public void Extension_ExportadorCsv_DevuelvePuntoCsv()
    {
        ExportadorCsv exportador = new ExportadorCsv();

        string extension = exportador.Extension;

        Assert.AreEqual(".csv", extension);
    }

    private static TablaExportableDto CrearTabla(List<string> encabezados)
    {
        TablaExportableDto tabla = new TablaExportableDto();
        tabla.Encabezados = encabezados;
        return tabla;
    }

    private static string LeerTexto(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes).TrimStart(MarcaDeOrdenDeBytes);
    }
}
