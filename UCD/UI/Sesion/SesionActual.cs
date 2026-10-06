using System.Security.Cryptography;
using DTOs;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Servicios;

namespace UI.Sesion;

// Guarda quien esta logueado. Como en el ejemplo del curso (BLAZOR/06-login), la sesion se guarda en el
// almacenamiento del navegador para que sobreviva a recargar la pagina; se usa la version protegida (cifrada)
// y solo se guarda el DNI, para que nadie pueda editarla desde el navegador. Los datos se traen del servidor.
public class SesionActual
{
    private const string ClaveDni = "dni_sesion";

    private readonly ProtectedLocalStorage _almacenamiento;
    private readonly ServicioSesion _servicioSesion;

    public SesionActual(ProtectedLocalStorage almacenamiento, ServicioSesion servicioSesion)
    {
        _almacenamiento = almacenamiento;
        _servicioSesion = servicioSesion;
    }

    public UsuarioSesionDto? Usuario { get; private set; }

    public bool EstaIniciada => Usuario is not null;

    // Indica si ya se leyo la sesion guardada en el navegador (las paginas esperan a que termine).
    public bool Cargada { get; private set; }

    // Aviso para la pantalla de ingreso cuando la sesion se cerro sin que el usuario saliera.
    public string? AvisoDeCierre { get; private set; }

    public event Action? Cambio;

    // Recupera la sesion guardada al abrir o recargar la pagina. Si el DNI guardado no es valido
    // (usuario desactivado o inexistente, o dato alterado), la sesion queda cerrada.
    public async Task CargarAsync()
    {
        if (Cargada)
        {
            return;
        }

        string? dni = await LeerDniGuardadoAsync();

        if (dni is not null)
        {
            try
            {
                Usuario = _servicioSesion.RenovarSesion(dni);
            }
            catch (ServicioException)
            {
                await _almacenamiento.DeleteAsync(ClaveDni);
            }
        }

        Cargada = true;
        Cambio?.Invoke();
    }

    public async Task IniciarAsync(UsuarioSesionDto usuario)
    {
        Usuario = usuario;
        AvisoDeCierre = null;
        await _almacenamiento.SetAsync(ClaveDni, usuario.Dni);
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

    public async Task CerrarAsync()
    {
        Usuario = null;
        await _almacenamiento.DeleteAsync(ClaveDni);
        Cambio?.Invoke();
    }

    // Cierra la sesion porque dejo de ser valida (por ejemplo, el usuario fue desactivado).
    public async Task ExpirarAsync(string aviso)
    {
        AvisoDeCierre = aviso;
        await CerrarAsync();
    }

    private async Task<string?> LeerDniGuardadoAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<string> resultado = await _almacenamiento.GetAsync<string>(ClaveDni);
            return resultado.Success ? resultado.Value : null;
        }
        catch (CryptographicException)
        {
            // El valor fue alterado o se cifro con otra clave: se descarta.
            await _almacenamiento.DeleteAsync(ClaveDni);
            return null;
        }
    }
}
