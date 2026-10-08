namespace DTOs;

public class CantidadesPorEstadoDto
{
    // Lo que agrupa la fila: el nombre del tipo en ContarPorTipo y el nombre de la categoria en ContarPorCategoria.
    public string Agrupador { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public int Disponibles { get; set; }

    public int Prestadas { get; set; }

    public int EnMantenimiento { get; set; }

    public int DadasDeBaja { get; set; }

    public int Total { get; set; }
}