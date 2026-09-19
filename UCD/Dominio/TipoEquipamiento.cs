namespace Dominio;

public class TipoEquipamiento
{
    public string Nombre { get; private set; }
    public string Descripcion  { get; private set; }
    public int DuracionMaximaDias { get; private set; }
    public bool RequiereAprobacion { get; private set; }
    public Categoria Categoria { get; private set; }
    public EstadoTipoEquipamiento Estado { get; private set; } = EstadoTipoEquipamiento.Activo;

        public TipoEquipamiento(string nombre, string descripcion, int duracionMaximaDias,
        bool requiereAprobacion, Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DominioException("El nombre del tipo de equipamiento no puede ser vacío");
        }
        if (nombre.Length < 3)
        {
            throw new DominioException("El nombre del tipo de equipamiento debe tener al menos 3 caracteres");
        }
        if (nombre.Length > 80)
        {
            throw new DominioException("El nombre del tipo de equipamiento no puede superar los 80 caracteres");
        }

        Nombre = nombre;
        Descripcion = descripcion;
        DuracionMaximaDias = duracionMaximaDias;
        RequiereAprobacion = requiereAprobacion;
        Categoria = categoria;
    }
}