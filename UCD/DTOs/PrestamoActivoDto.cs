namespace DTOs;

public class PrestamoActivoDto
{
    public Guid Id { get; set; }
    public string Solicitante { get; set; } = string.Empty;
    public string TipoEquipamiento { get; set; } = string.Empty;
    public DateTime FechaHoraPrestamo { get; set; }
    public List<string> Unidades { get; set; } = new List<string>();
}
