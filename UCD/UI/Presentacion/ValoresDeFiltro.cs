namespace UI.Presentacion;

public static class ValoresDeFiltro
{
    // En los filtros, "Todas" se representa con texto vacio y para el servicio significa no filtrar (null).
    public static string? ValorONulo(string valor)
    {
        if (string.IsNullOrEmpty(valor))
        {
            return null;
        }

        return valor;
    }
}