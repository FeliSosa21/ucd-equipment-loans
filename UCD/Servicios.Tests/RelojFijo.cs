namespace Servicios.Tests;

public class RelojFijo : TimeProvider
{
    private readonly DateTimeOffset _instante;

    public RelojFijo(DateTime fechaHora)
    {
        _instante = new DateTimeOffset(fechaHora, TimeSpan.Zero);
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _instante;
    }

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}