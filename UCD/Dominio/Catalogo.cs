namespace Dominio;

public class Catalogo
{
    private const int LargoMinimoNombre = 3;
    private const int LargoMaximoNombre = 80;
        
    public string Nombre { get; private set; }

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
}