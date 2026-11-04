using System.Text;
using DTOs;

namespace Servicios;

public class ExportadorCsv : IExportadorDeTabla
{
    private const string Separador = ",";
    private const string FinDeRegistro = "\r\n";
    private const string Comillas = "\"";

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
        string texto = EscribirRegistro(tabla.Encabezados);
        foreach (List<string> fila in tabla.Filas)
        {
            texto += EscribirRegistro(fila);
        }
        return Encoding.UTF8.GetBytes(texto);
    }

    private static string EscribirRegistro(List<string> campos)
    {
        List<string> camposEscritos = new List<string>();
        foreach (string campo in campos)
        {
            camposEscritos.Add(EscribirCampo(campo));
        }
        return string.Join(Separador, camposEscritos) + FinDeRegistro;
    }

    private static string EscribirCampo(string campo)
    {
        if (campo.Contains(Separador))
        {
            return Comillas + campo + Comillas;
        }
        return campo;
    }
}
