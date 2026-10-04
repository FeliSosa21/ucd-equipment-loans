using DTOs;

namespace UI.Sesion;

// Guarda quien esta logueado mientras dure la conexion del navegador (circuito de Blazor).
public class SesionActual
{
    public UsuarioSesionDto? Usuario { get; private set; }

    public bool EstaIniciada => Usuario is not null;

    // Aviso para la pantalla de ingreso cuando la sesion se cerro sin que el usuario saliera.
    public string? AvisoDeCierre { get; private set; }

    public event Action? Cambio;

    public void Iniciar(UsuarioSesionDto usuario)
    {
        Usuario = usuario;
        AvisoDeCierre = null;
        Cambio?.Invoke();
    }

    // Reemplaza los datos guardados por los actuales (tipo, permisos, etc.).
    public void Renovar(UsuarioSesionDto usuario)
    {
        Usuario = usuario;
        Cambio?.Invoke();
    }

    public void MarcarContrasenaCambiada()
    {
        if (Usuario is null)
        {
            return;
        }

        Usuario.DebeCambiarContrasena = false;
        Cambio?.Invoke();
    }

    public void ActualizarDatos(string nombre, string correo)
    {
        if (Usuario is null)
        {
            return;
        }

        Usuario.Nombre = nombre;
        Usuario.Correo = correo;
        Cambio?.Invoke();
    }

    public void Cerrar()
    {
        Usuario = null;
        Cambio?.Invoke();
    }

    // Cierra la sesion porque dejo de ser valida (por ejemplo, el usuario fue desactivado).
    public void Expirar(string aviso)
    {
        AvisoDeCierre = aviso;
        Cerrar();
    }
}
