namespace DTOs;

// Una fila del inventario (req. 9): cuantas unidades hay en cada estado,
// para un tipo de equipamiento o para una categoria.
public class CantidadesPorEstadoDto
{
    public string Nombre { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public int Disponibles { get; set; }

    public int Prestadas { get; set; }

    public int EnMantenimiento { get; set; }

    public int DadasDeBaja { get; set; }

    public int Total { get; set; }
}