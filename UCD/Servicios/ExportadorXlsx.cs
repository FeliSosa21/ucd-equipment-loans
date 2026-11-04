using ClosedXML.Excel;
using DTOs;

namespace Servicios;

public class ExportadorXlsx : IExportadorDeTabla
{
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
        libro.Worksheets.Add("Hoja");
        using MemoryStream flujo = new MemoryStream();
        libro.SaveAs(flujo);
        return flujo.ToArray();
    }
}
