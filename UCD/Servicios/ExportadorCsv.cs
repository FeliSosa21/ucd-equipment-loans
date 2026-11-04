using System.Text;
using DTOs;

namespace Servicios;

public class ExportadorCsv : IExportadorDeTabla
{
    private const string Separador = ",";
    private const string FinDeRegistro = "\r\n";
    private const string Comillas = "\"";
    private const string SaltoDeLinea = "\n";

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
        ArgumentNullException.ThrowIfNull(tabla);
        ValidarQueNingunaFilaSuperaLosEncabezados(tabla);
        string texto = EscribirRegistro(tabla.Encabezados);
        foreach (List<string> fila in tabla.Filas)
        {
            texto += EscribirRegistro(fila);
        }
        UTF8Encoding codificacion = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        byte[] marcaDeOrdenDeBytes = codificacion.GetPreamble();
        byte[] contenido = codificacion.GetBytes(texto);
        return marcaDeOrdenDeBytes.Concat(contenido).ToArray();
    }

    private static void ValidarQueNingunaFilaSuperaLosEncabezados(TablaExportableDto tabla)
    {
        foreach (List<string> fila in tabla.Filas)
        {
            if (fila.Count > tabla.Encabezados.Count)
            {
                throw new ArgumentException(
                    "Una fila tiene " + fila.Count + " columnas y el encabezado solo " + tabla.Encabezados.Count);
            }
        }
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
        if (campo.Contains(Separador) || campo.Contains(Comillas) || campo.Contains(SaltoDeLinea))
        {
            string campoConComillasDuplicadas = campo.Replace(Comillas, Comillas + Comillas);
            return Comillas + campoConComillasDuplicadas + Comillas;
        }
        return campo;
    }
}
