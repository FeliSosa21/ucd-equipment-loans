namespace DTOs;

public class DatosTipoEquipamientoDto
{
    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public int DuracionMaxPrestamo { get; set; }

    public bool RequiereAprobacion { get; set; }

    public string Categoria { get; set; } = string.Empty;
}
