using System.Text;
using DTOs;

namespace Servicios.Tests;

// Exportador falso: se queda con las tablas que recibe para que los tests miren las filas sin abrir un archivo.
internal sealed class ExportadorDePrueba : IExportadorDeTabla
{
    public List<TablaExportableDto> TablasRecibidas { get; } = new List<TablaExportableDto>();

    public string Formato => "TXT";
    public string Extension => ".txt";
    public string TipoContenido => "text/plain";

    public byte[] Exportar(TablaExportableDto tabla)
    {
        TablasRecibidas.Add(tabla);
        return Encoding.UTF8.GetBytes(string.Join("|", tabla.Encabezados));
    }
}
