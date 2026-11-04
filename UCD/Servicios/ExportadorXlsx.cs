using ClosedXML.Excel;
using DTOs;

namespace Servicios;

public class ExportadorXlsx : IExportadorDeTabla
{
    private const string NombreDeHoja = "Exportacion";
    private const int FilaDeEncabezados = 1;

    public string Formato
    {
        get { return "XLSX"; }
    }

    public string Extension
    {
        get { return ".xlsx"; }
    }

    public string TipoContenido
    {
        get { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
    }

    public byte[] Exportar(TablaExportableDto tabla)
    {
        ArgumentNullException.ThrowIfNull(tabla);
        using XLWorkbook libro = new XLWorkbook();
        IXLWorksheet hoja = libro.Worksheets.Add(NombreDeHoja);
        EscribirEncabezados(hoja, tabla);
        EscribirFilas(hoja, tabla);
        return GuardarComoBytes(libro);
    }

    private static void EscribirEncabezados(IXLWorksheet hoja, TablaExportableDto tabla)
    {
        EscribirRegistro(hoja, FilaDeEncabezados, tabla.Encabezados);
    }

    private static void EscribirFilas(IXLWorksheet hoja, TablaExportableDto tabla)
    {
        for (int fila = 0; fila < tabla.Filas.Count; fila++)
        {
            int numeroDeFila = FilaDeEncabezados + fila + 1;
            EscribirRegistro(hoja, numeroDeFila, tabla.Filas[fila]);
        }
    }

    private static void EscribirRegistro(IXLWorksheet hoja, int numeroDeFila, List<string> campos)
    {
        for (int columna = 0; columna < campos.Count; columna++)
        {
            hoja.Cell(numeroDeFila, columna + 1).Value = campos[columna];
        }
    }

    private static byte[] GuardarComoBytes(XLWorkbook libro)
    {
        using MemoryStream flujo = new MemoryStream();
        libro.SaveAs(flujo);
        return flujo.ToArray();
    }
}
