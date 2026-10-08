namespace DTOs;

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