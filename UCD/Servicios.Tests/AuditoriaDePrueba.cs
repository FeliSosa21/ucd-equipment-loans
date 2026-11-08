using Data;
using Data.Repositorios;

namespace Servicios.Tests;

// Registrador que guarda en la misma Memoria del test, con un reloj fijo: el test puede leer el log con
// un RepositorioAuditoria sobre esa Memoria.
internal static class AuditoriaDePrueba
{
    public static readonly DateTime Ahora = new(2026, 11, 8, 9, 30, 0);

    public static RegistradorDeAuditoria Registrador(Memoria memoria)
    {
        return new RegistradorDeAuditoria(new RepositorioAuditoria(memoria), new RelojFijo(Ahora));
    }
}
