namespace DTOs;

public class PrestamoDto
{
    public Guid Id { get; set; }
    public string Solicitante { get; set; } = string.Empty;
    public string TipoEquipamiento { get; set; } = string.Empty;
    public int CantidadSolicitada { get; set; }
    public DateTime FechaHoraPrestamo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MotivoRechazo { get; set; }
    public List<string> Unidades { get; set; } = new List<string>();
}