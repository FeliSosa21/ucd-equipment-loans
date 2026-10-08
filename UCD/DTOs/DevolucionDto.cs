namespace DTOs;

public class DevolucionDto
{
    public Guid IdPrestamo { get; set; }
    public List<UnidadDevueltaDto> Unidades { get; set; } = new List<UnidadDevueltaDto>();
}
