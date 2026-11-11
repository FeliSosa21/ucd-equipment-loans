using Dominio;

namespace Servicios.Tests;

// Datos de un prestamo (y de su devolucion, si la hay) que ContextoDePrueba guarda: evita metodos con muchos parametros.
internal sealed class RegistroDePrueba
{
    public int IdCatalogo { get; init; }
    public string NombreTipo { get; init; } = string.Empty;
    public string DniSolicitante { get; init; } = string.Empty;
    public DateTime FechaPrestamo { get; init; }
    public string DniEvaluador { get; init; } = string.Empty;
    public string DniQuienRegistra { get; init; } = string.Empty;
    public DateTime FechaDevolucion { get; init; }
    public CondicionDevolucion Condicion { get; init; } = CondicionDevolucion.SinObservaciones;
}
