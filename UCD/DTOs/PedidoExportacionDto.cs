namespace DTOs;

public class PedidoExportacionDto
{
    public RangoDeExportacionDto Rango { get; set; } = new RangoDeExportacionDto();
    public string Formato { get; set; } = string.Empty;
}
