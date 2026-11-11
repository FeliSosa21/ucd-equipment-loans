namespace Servicios.Tests;

// Cada lectura devuelve un minuto mas que la anterior: si una operacion lee el reloj dos veces, el test lo ve.
internal sealed class RelojQueAvanza : TimeProvider
{
    private DateTimeOffset _instante;

    public RelojQueAvanza(DateTime inicio)
    {
        _instante = new DateTimeOffset(inicio, TimeSpan.Zero);
    }

    public override DateTimeOffset GetUtcNow()
    {
        DateTimeOffset actual = _instante;
        _instante = _instante.AddMinutes(1);

        return actual;
    }

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
