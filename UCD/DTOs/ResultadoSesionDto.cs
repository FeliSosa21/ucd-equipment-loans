namespace DTOs;

// Resultado de intentar iniciar sesion: credenciales incorrectas o usuario desactivado son resultados
// esperables, no errores, por eso se informan con Exitoso y Mensaje en lugar de una excepcion.
public class ResultadoSesionDto
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public UsuarioSesionDto? Sesion { get; set; }
}
