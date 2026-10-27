namespace Dominio;

public class Catalogo
{
    private const int LargoMinimoNombre = 3;
    private const int LargoMaximoNombre = 80;

    private readonly List<TipoEquipamiento> _tipos = new();
    public string Nombre { get; private set; }
    public int Id { get; private set; }

    public IReadOnlyCollection<TipoEquipamiento> Tipos => _tipos.AsReadOnly();

    private Catalogo()
    {
        Nombre = null!;
    }

    public Catalogo(string nombre)
    {
        ValidarNombre(nombre);
        Nombre = nombre;
    }

    public void ModificarNombre(string nombre)
    {
        ValidarNombre(nombre);
        Nombre = nombre;
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DominioException("El nombre del catalogo no puede ser vacio");
        }

        if (nombre.Length < LargoMinimoNombre)
        {
            throw new DominioException($"El nombre del catalogo debe tener al menos {LargoMinimoNombre} caracteres");
        }

        if (nombre.Length > LargoMaximoNombre)
        {
            throw new DominioException($"El nombre del catalogo debe tener como maximo {LargoMaximoNombre} caracteres");
        }
    }

    public void AgregarTipo(TipoEquipamiento tipo)
    {
        if (tipo is null)
        {
            throw new DominioException("El tipo no puede ser nulo");
        }

        if (ExisteTipoConNombre(tipo.Nombre))
        {
            throw new DominioException("Ya existe un tipo de equipamiento con ese nombre");
        }

        _tipos.Add(tipo);
    }

    private bool ExisteTipoConNombre(string nombre)
    {
        return BuscarTipoPorNombre(nombre) is not null;
    }

    public TipoEquipamiento? BuscarTipoPorNombre(string nombre)
    {
        var nombreBuscado = nombre.Trim();

        foreach (var tipo in _tipos)
        {
            if (string.Equals(tipo.Nombre.Trim(), nombreBuscado, StringComparison.OrdinalIgnoreCase))
            {
                return tipo;
            }
        }

        return null;
    }
}