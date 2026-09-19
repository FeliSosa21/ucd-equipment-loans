namespace Dominio;

public class TipoEquipamiento
{
    public EstadoTipoEquipamiento Estado { get; private set; } = EstadoTipoEquipamiento.Activo;

    public TipoEquipamiento(string nombre, string descripcion, int duracionMaximaDias,
        bool requiereAprobacion, Categoria categoria)
    {
    }
}