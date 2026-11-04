namespace DTOs;

public class TipoUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
    public int? MaxUnidadesPorCatalogo { get; set; }
    public int? MaxPrestamosConcurrentes { get; set; }
}
