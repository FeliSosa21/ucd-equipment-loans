namespace Dominio;

public class Catalogo {
    
    public string Nombre { get; private set; }
    public Catalogo(string nombre) {
        Nombre = nombre;
    }
    public void ModificarNombre(string nombre) {
        Nombre = nombre;
    }
    
}