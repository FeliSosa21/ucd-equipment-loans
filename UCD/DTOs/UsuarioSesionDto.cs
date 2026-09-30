namespace DTOs;

public class UsuarioSesionDto
{
    public string Dni { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool DebeCambiarContrasena { get; set; }
    public bool PendienteDeActivacion { get; set; }
    public List<string> Permisos { get; set; } = new List<string>();
}
