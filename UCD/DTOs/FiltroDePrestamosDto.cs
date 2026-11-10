namespace DTOs;

public class FiltroDePrestamosDto
{
    public required DateTime Desde { get; set; }

    public required DateTime Hasta { get; set; }

    public string? Estado { get; set; }
}
