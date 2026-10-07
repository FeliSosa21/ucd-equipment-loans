namespace Dominio;

public static class CalendarioLaboral
{
    public static DateTime SumarDiasHabiles(DateTime desde, int diasHabiles)
    {
        DateTime fecha = desde.Date;
        int diasHabilesContados = 0;

        while (diasHabilesContados < diasHabiles)
        {
            fecha = fecha.AddDays(1);

            if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
            {
                diasHabilesContados++;
            }
        }

        return fecha;
    }
}