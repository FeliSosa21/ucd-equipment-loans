namespace DTOs;

public class UsuarioSesionDto
{
    public string Correo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool DebeCambiarContrasena { get; set; }
}
