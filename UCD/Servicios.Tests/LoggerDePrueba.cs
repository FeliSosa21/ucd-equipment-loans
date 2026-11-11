using Microsoft.Extensions.Logging;

namespace Servicios.Tests;

// Anota el nivel y la excepcion de cada registro, para comprobar que una falla quedo registrada.
internal sealed class LoggerDePrueba<TCategoria> : ILogger<TCategoria>
{
    public List<(LogLevel Nivel, Exception? Excepcion)> Registros { get; } =
        new List<(LogLevel Nivel, Exception? Excepcion)>();

    public IDisposable? BeginScope<TEstado>(TEstado estado) where TEstado : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel nivel)
    {
        return true;
    }

    public void Log<TEstado>(LogLevel nivel, EventId idDeEvento, TEstado estado, Exception? excepcion,
        Func<TEstado, Exception?, string> formateador)
    {
        Registros.Add((nivel, excepcion));
    }
}
