using System.Text;
using DTOs;

namespace Servicios;

public class ExportadorCsv : IExportadorDeTabla
{
    private const string Separador = ",";
    private const string FinDeRegistro = "\r\n";

    public string Formato
    {
        get { return "CSV"; }
    }

    public string Extension
    {
        get { return ".csv"; }
    }

    public string TipoContenido
    {
        get { return "text/csv"; }
    }

    public byte[] Exportar(TablaExportableDto tabla)
    {
        string texto = string.Join(Separador, tabla.Encabezados) + FinDeRegistro;
        return Encoding.UTF8.GetBytes(texto);
    }
}
