namespace DTOs;

public class DevolucionRegistradaDto
{
    public string RegistradaPor { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public List<UnidadDevueltaDto> Unidades { get; set; } = new List<UnidadDevueltaDto>();
}
