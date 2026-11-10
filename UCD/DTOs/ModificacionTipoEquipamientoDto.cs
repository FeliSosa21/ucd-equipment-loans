namespace DTOs;

public class ModificacionTipoEquipamientoDto
{
    public string NombreActual { get; set; } = string.Empty;

    public DatosTipoEquipamientoDto Datos { get; set; } = new DatosTipoEquipamientoDto();
}
