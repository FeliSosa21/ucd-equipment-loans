using System.Text;
using DTOs;

namespace Servicios;

public class ExportadorCsv : IExportadorDeTabla
{
    private const string Separador = ",";
    private const string FinDeRegistro = "\r\n";

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
        string texto = string.Join(Separador, tabla.Encabezados) + FinDeRegistro;
        return Encoding.UTF8.GetBytes(texto);
    }
}
