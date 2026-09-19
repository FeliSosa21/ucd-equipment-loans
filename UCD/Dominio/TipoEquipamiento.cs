namespace Dominio;

public class TipoEquipamiento
{
    public string Nombre { get; private set; }
    public string Descripcion  { get; private set; }
    public int DuracionMaximaDias => throw new NotImplementedException();
    public EstadoTipoEquipamiento Estado { get; private set; } = EstadoTipoEquipamiento.Activo;

    public TipoEquipamiento(string nombre, string descripcion, int duracionMaximaDias,
        bool requiereAprobacion, Categoria categoria)
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }
}