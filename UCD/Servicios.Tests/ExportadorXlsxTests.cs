using ClosedXML.Excel;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ExportadorXlsxTests
{
    [TestMethod]
    public void Exportar_TablaSinFilas_EscribeUnaHojaConSoloElEncabezado()
    {
        ExportadorXlsx exportador = new ExportadorXlsx();
        TablaExportableDto tabla = CrearTabla(encabezados: new List<string> { "Nombre", "Edad" });

        byte[] bytes = exportador.Exportar(tabla);

        using XLWorkbook libro = AbrirLibro(bytes);
        IXLWorksheet hoja = libro.Worksheet(1);
        Assert.AreEqual("Nombre", hoja.Cell(1, 1).GetString());
        Assert.AreEqual("Edad", hoja.Cell(1, 2).GetString());
        Assert.AreEqual(1, libro.Worksheets.Count);
        Assert.AreEqual(1, hoja.LastRowUsed()!.RowNumber());
    }

    [TestMethod]
    public void Exportar_TablaConUnaFila_EscribeLosValoresDeLaFilaComoTexto()
    {
        ExportadorXlsx exportador = new ExportadorXlsx();
        TablaExportableDto tabla = CrearTabla(
            encabezados: new List<string> { "Nombre", "Edad" },
            filas: new List<List<string>> { new List<string> { "Ana", "20" } });

        byte[] bytes = exportador.Exportar(tabla);

        using XLWorkbook libro = AbrirLibro(bytes);
        IXLWorksheet hoja = libro.Worksheet(1);
        Assert.AreEqual("Ana", hoja.Cell(2, 1).GetString());
        Assert.AreEqual("20", hoja.Cell(2, 2).GetString());
        Assert.AreEqual(XLDataType.Text, hoja.Cell(2, 2).DataType);
        Assert.AreEqual(2, hoja.LastRowUsed()!.RowNumber());
    }

    private static TablaExportableDto CrearTabla(List<string> encabezados, List<List<string>>? filas = null)
    {
        TablaExportableDto tabla = new TablaExportableDto();
        tabla.Encabezados = encabezados;
        if (filas != null)
        {
            tabla.Filas = filas;
        }
        return tabla;
    }

    private static XLWorkbook AbrirLibro(byte[] bytes)
    {
        MemoryStream flujo = new MemoryStream(bytes);
        return new XLWorkbook(flujo);
    }
}
