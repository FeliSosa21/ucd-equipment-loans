namespace Dominio;

public static class CalendarioLaboral
{
    public static DateTime SumarDiasHabiles(DateTime desde, int diasHabiles)
    {
        return desde.Date.AddDays(diasHabiles);
    }
}