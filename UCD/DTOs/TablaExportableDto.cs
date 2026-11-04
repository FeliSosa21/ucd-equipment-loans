namespace DTOs;

public class TablaExportableDto
{
    public List<string> Encabezados { get; set; } = new List<string>();
    public List<List<string>> Filas { get; set; } = new List<List<string>>();
}
