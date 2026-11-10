namespace Data.Repositorios;

public class FiltroPorCatalogoYFechas
{
    public int? IdCatalogo { get; init; }
    public DateTime? Desde { get; init; }
    public DateTime? Hasta { get; init; }

    public bool TieneCriterios()
    {
        throw new NotImplementedException();
    }
}
