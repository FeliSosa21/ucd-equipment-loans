namespace Dominio;

public class Catalogo
{
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

    private void ValidarNombre(string nombre)
    {
        if (nombre == null)
        {
            throw new ArgumentException("El nombre del catalogo no debe ser nulo");
        }
        if(nombre.Length<3)
        {
            throw new ArgumentException("El nombre del catalogo debe tener al menos 3 caracteres");
        }
        if(nombre.Length>80)
        {
            throw new ArgumentException("El nombre del catalogo debe tener menos de 81 caracteres");
        }
        
    }
    
}