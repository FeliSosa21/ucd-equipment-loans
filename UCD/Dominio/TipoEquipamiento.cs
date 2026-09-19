namespace Dominio;

public class TipoEquipamiento
{
    public string Nombre => throw new NotImplementedException();
    public EstadoTipoEquipamiento Estado { get; private set; } = EstadoTipoEquipamiento.Activo;

    public TipoEquipamiento(string nombre, string descripcion, int duracionMaximaDias,
        bool requiereAprobacion, Categoria categoria)
    {
    }
}