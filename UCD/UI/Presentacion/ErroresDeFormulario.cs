using Servicios;

namespace UI.Presentacion;

// Errores de un formulario: los de un campo que el formulario muestra van junto a ese campo; el resto
// (sin campo, o de un campo que este formulario no tiene) va al aviso general para que no se pierda.
public class ErroresDeFormulario
{
    private readonly Dictionary<string, string> _porCampo = new Dictionary<string, string>();
    private readonly HashSet<string> _camposDelFormulario;

    public ErroresDeFormulario(params string[] camposDelFormulario)
    {
        _camposDelFormulario = new HashSet<string>(camposDelFormulario);
    }

    public string? General { get; private set; }

    public void Agregar(ServicioException excepcion)
    {
        if (!Muestra(excepcion.Campo))
        {
            General = excepcion.Message;
            return;
        }

        _porCampo[excepcion.Campo!] = excepcion.Message;
    }

    public bool Muestra(string? campo) => campo is not null && _camposDelFormulario.Contains(campo);

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
