using ClosedXML.Excel;
using DTOs;

namespace Servicios;

public class ExportadorXlsx : IExportadorDeTabla
{
    private const string NombreDeHoja = "Exportacion";
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
        ArgumentNullException.ThrowIfNull(tabla);
        using XLWorkbook libro = new XLWorkbook();
        IXLWorksheet hoja = libro.Worksheets.Add(NombreDeHoja);
        for (int columna = 0; columna < tabla.Encabezados.Count; columna++)
        {
            hoja.Cell(FilaDeEncabezados, columna + 1).Value = tabla.Encabezados[columna];
        }
        for (int fila = 0; fila < tabla.Filas.Count; fila++)
        {
            for (int columna = 0; columna < tabla.Filas[fila].Count; columna++)
            {
                hoja.Cell(FilaDeEncabezados + fila + 1, columna + 1).Value = tabla.Filas[fila][columna];
            }
        }
        using MemoryStream flujo = new MemoryStream();
        libro.SaveAs(flujo);
        return flujo.ToArray();
    }
}
