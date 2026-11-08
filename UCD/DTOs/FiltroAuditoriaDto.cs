namespace DTOs;

public class FiltroAuditoriaDto
{
    public DateOnly? Desde { get; set; }

    public DateOnly? Hasta { get; set; }

    public string? DniUsuario { get; set; }
}
