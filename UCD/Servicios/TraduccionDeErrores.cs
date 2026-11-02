using Dominio;

namespace Servicios;

// Al cruzar de capa, los errores del dominio llegan a la UI como ServicioException (con la original como interna).
internal static class TraduccionDeErrores
{
    public static void Ejecutar(Action operacion)
    {
        try
        {
            operacion();
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Campo, ex.Message, ex);
        }
    }

    public static T Ejecutar<T>(Func<T> operacion)
    {
        try
        {
            return operacion();
        }
        catch (DominioException ex)
        {
            throw new ServicioException(ex.Campo, ex.Message, ex);
        }
    }
}
