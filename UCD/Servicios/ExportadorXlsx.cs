using ClosedXML.Excel;
using DTOs;

namespace Servicios;

public class ExportadorXlsx : IExportadorDeTabla
{
    private const int FilaDeEncabezados = 1;

    public string Formato
    {
        get { return string.Empty; }
    }

    public string Extension
    {
        get { return string.Empty; }
    }

    public string TipoContenido
    {
        get { return string.Empty; }
    }

    public byte[] Exportar(TablaExportableDto tabla)
    {
        using XLWorkbook libro = new XLWorkbook();
        IXLWorksheet hoja = libro.Worksheets.Add("Hoja");
        for (int columna = 0; columna < tabla.Encabezados.Count; columna++)
        {
            hoja.Cell(FilaDeEncabezados, columna + 1).Value = tabla.Encabezados[columna];
        }
        using MemoryStream flujo = new MemoryStream();
        libro.SaveAs(flujo);
        return flujo.ToArray();
    }
}
