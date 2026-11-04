using DTOs;

namespace Servicios;

public class ExportadorCsv : IExportadorDeTabla
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
        return Array.Empty<byte>();
    }
}
