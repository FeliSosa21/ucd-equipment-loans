namespace Dominio;

public class TopesDePrestamo
{
    public int MaxUnidadesPorCatalogo { get; private set; }
    public int MaxPrestamosConcurrentes { get; private set; }

    public TopesDePrestamo(int maxUnidadesPorCatalogo, int maxPrestamosConcurrentes)
    {
        ValidarTopes(maxUnidadesPorCatalogo, maxPrestamosConcurrentes);

        MaxUnidadesPorCatalogo = maxUnidadesPorCatalogo;
        MaxPrestamosConcurrentes = maxPrestamosConcurrentes;
    }

    public void Modificar(int maxUnidadesPorCatalogo, int maxPrestamosConcurrentes)
    {
        ValidarTopes(maxUnidadesPorCatalogo, maxPrestamosConcurrentes);

        MaxUnidadesPorCatalogo = maxUnidadesPorCatalogo;
        MaxPrestamosConcurrentes = maxPrestamosConcurrentes;
    }

    public bool PuedeAgregarPrestamo(int prestamosVigentes)
    {
        return prestamosVigentes < MaxPrestamosConcurrentes;
    }

    public bool PermiteUnidades(int cantidadSolicitada)
    {
        return cantidadSolicitada <= MaxUnidadesPorCatalogo;
    }

    private static void ValidarTopes(int maxUnidadesPorCatalogo, int maxPrestamosConcurrentes)
    {
        ValidarTope(maxUnidadesPorCatalogo, "maximo de unidades por catalogo");
        ValidarTope(maxPrestamosConcurrentes, "maximo de prestamos concurrentes");
    }

    private static void ValidarTope(int tope, string campo)
    {
        if (tope < 0)
        {
            throw new DominioException($"El {campo} no puede ser negativo");
        }
    }
}
