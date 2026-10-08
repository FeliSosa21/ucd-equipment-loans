namespace UI.Presentacion;

// Estados de tipos y unidades tal como llegan en los DTOs (la UI no conoce los enums del dominio).
public static class EstadosCatalogo
{
    public const string TipoActivo = "Activo";
    public const string TipoDiscontinuado = "Discontinuado";

    public const string UnidadDisponible = "Disponible";
    public const string UnidadPrestada = "Prestada";
    public const string UnidadEnMantenimiento = "EnMantenimiento";
    public const string UnidadDadaDeBaja = "DadaDeBaja";
}
