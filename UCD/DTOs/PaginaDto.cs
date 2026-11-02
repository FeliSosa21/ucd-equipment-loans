namespace DTOs;

public class PaginaDto<T>
{
    public List<T> Elementos { get; set; } = new List<T>();
    public int Pagina { get; set; }
    public bool HayPaginaAnterior { get; set; }
    public bool HayPaginaSiguiente { get; set; }
}
