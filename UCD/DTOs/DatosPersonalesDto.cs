namespace DTOs;

// Datos que un usuario (o un administrador) puede modificar de una cuenta.
public class DatosPersonalesDto
{
    public string Dni { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
}
