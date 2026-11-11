namespace DTOs;

// Fechas elegidas en la pantalla: pueden venir vacias, el servicio lo informa junto al campo.
public class RangoDeExportacionDto
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
}
