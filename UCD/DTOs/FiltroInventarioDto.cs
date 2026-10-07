namespace DTOs;

// null = no filtrar por ese criterio (mismo criterio que FiltroCatalogoDto).
public class FiltroInventarioDto
{
    public string? Categoria { get; set; }

    public string? Estado { get; set; }
}