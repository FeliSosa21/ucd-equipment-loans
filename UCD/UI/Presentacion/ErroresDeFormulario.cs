using Servicios;

namespace UI.Presentacion;

// Errores de un formulario: los que vienen con campo se muestran junto a ese campo; el resto, en un aviso general.
public class ErroresDeFormulario
{
    private readonly Dictionary<string, string> _porCampo = new Dictionary<string, string>();

    public string? General { get; private set; }

    public void Agregar(ServicioException excepcion)
    {
        if (excepcion.Campo is null)
        {
            General = excepcion.Message;
            return;
        }

        _porCampo[excepcion.Campo] = excepcion.Message;
    }

    public void AgregarEnCampo(string campo, string mensaje)
    {
        _porCampo[campo] = mensaje;
    }

    public void Limpiar()
    {
        _porCampo.Clear();
        General = null;
    }

    public void QuitarDe(string campo)
    {
        _porCampo.Remove(campo);
    }

    public bool Tiene(string campo) => _porCampo.ContainsKey(campo);

    // Blazor escribe un bool true como atributo vacio (aria-invalid=""), que los lectores de pantalla toman como
    // falso: por eso se devuelve el texto "true", o null para que no se escriba el atributo.
    public string? AriaInvalid(string campo) => Tiene(campo) ? "true" : null;

    public string? De(string campo) => _porCampo.GetValueOrDefault(campo);

    // Id del mensaje para aria-describedby; null si el campo no tiene error.
    public string? IdDelError(string campo, string idDelCampo) => Tiene(campo) ? IdDelMensaje(idDelCampo) : null;

    public static string IdDelMensaje(string idDelCampo) => $"error-{idDelCampo}";
}
