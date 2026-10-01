namespace DTOs;

public class TipoEquipamientoDto
{
    public string Nombre { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public int CantidadDisponibles { get; set; } = 0;
}