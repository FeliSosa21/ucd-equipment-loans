namespace Dominio;

public class Catalogo
{
    private const int LargoMinimoNombre = 3;
    private const int LargoMaximoNombre = 80;

    public string Nombre { get; private set; }
    private readonly List<TipoEquipamiento> _tipos = new();

    public IReadOnlyCollection<TipoEquipamiento> Tipos => _tipos.AsReadOnly();

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

    static void ValidarNombre(string nombre)
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
        if (BuscarTipoPorNombre(nombre) is not null)
        {
            return true;
        }

        return false;
    }

    public TipoEquipamiento? BuscarTipoPorNombre(string nombre)
    {
        string nombreSinEspacios = nombre.Trim();
        foreach (var tipo in _tipos)
        {
            if (string.Equals(tipo.Nombre.Trim(), nombreSinEspacios, StringComparison.OrdinalIgnoreCase))
            {
                return tipo;
            }
        }

        return null;
    }
}