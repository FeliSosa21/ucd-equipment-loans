namespace UI.Presentacion;

public static class TextosVisibles
{
    public static readonly IReadOnlyDictionary<string, string> Categorias = new Dictionary<string, string>
    {
        { "Computacion", "Computación" },
        { "Audiovisual", "Audiovisual" },
        { "Laboratorio", "Laboratorio" },
        { "Electronica", "Electrónica" },
        { "Accesorios", "Accesorios" }
    };

    public static readonly IReadOnlyDictionary<string, string> EstadosDeUnidad = new Dictionary<string, string>
    {
        { "Disponible", "Disponible" },
        { "Prestada", "Prestada" },
        { "EnMantenimiento", "En mantenimiento" },
        { "DadaDeBaja", "Dada de baja" }
    };

    public static string Categoria(string valor)
    {
        if (Categorias.ContainsKey(valor))
        {
            return Categorias[valor];
        }

        return valor;
    }
}