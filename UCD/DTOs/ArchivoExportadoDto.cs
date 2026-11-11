namespace DTOs;

public class ArchivoExportadoDto
{
    public string Nombre { get; set; } = string.Empty;
    public byte[] Contenido { get; set; } = Array.Empty<byte>();
    public string TipoContenido { get; set; } = string.Empty;
}
