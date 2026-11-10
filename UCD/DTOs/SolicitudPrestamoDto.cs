namespace DTOs;

public class SolicitudPrestamoDto
{
    public int IdCatalogo { get; set; }
    public string TipoEquipamiento { get; set; } = string.Empty;
    public int CantidadSolicitada { get; set; }
}