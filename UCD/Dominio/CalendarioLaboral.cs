namespace Dominio;

public static class CalendarioLaboral
{
    private static readonly (int Mes, int Dia)[] FeriadosNoLaborables = {
        (1, 1),
        (5, 1),
        (7, 18),
        (8, 25),
        (12, 25)
    };

    public static DateTime SumarDiasHabiles(DateTime desde, int diasHabiles)
    {
        ValidarDiasHabiles(diasHabiles);

        DateTime fecha = desde.Date;
        int diasHabilesContados = 0;

        while (diasHabilesContados < diasHabiles)
        {
            fecha = fecha.AddDays(1);

            if (fecha.DayOfWeek != DayOfWeek.Saturday 
                && fecha.DayOfWeek != DayOfWeek.Sunday
                && !FeriadosNoLaborables.Contains((fecha.Month, fecha.Day)))
            {
                diasHabilesContados++;
            }
        }

        return fecha;
    }

    private static void ValidarDiasHabiles(int diasHabiles)
    {
        if (diasHabiles <= 0)
        {
            throw new DominioException("La cantidad de dias habiles debe ser mayor a cero");
        }
    }
}