namespace Dominio;

public class TipoEquipamiento
{
    public EstadoTipoEquipamiento Estado => throw new NotImplementedException();

    public TipoEquipamiento(string nombre, string descripcion, int duracionMaximaDias,
        bool requiereAprobacion, Categoria categoria)
    {
    }
}