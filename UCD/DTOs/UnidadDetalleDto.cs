namespace DTOs;

public class UnidadDetalleDto
{
    public string IdInstitucional { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string UbicacionFisica { get; set; } = string.Empty;

    public string? NumeroSerie { get; set; }

    public string? Observaciones { get; set; }

    public string Estado { get; set; } = string.Empty;
}