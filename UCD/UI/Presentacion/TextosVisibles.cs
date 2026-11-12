namespace UI.Presentacion;

public static class TextosVisibles
{
    public static readonly IReadOnlyDictionary<string, string> Categorias = new Dictionary<string, string>
    {
        { "Computacion", "Computación" },
        { "Audiovisual", "Audiovisual" },
        { "Laboratorio", "Laboratorio" },
        { "Electronica", "Electrónica" },
        { "Accesorios", "Accesorios" }
    };

    public static readonly IReadOnlyDictionary<string, string> EstadosDeUnidad = new Dictionary<string, string>
    {
        { "Disponible", "Disponible" },
        { "Prestada", "Prestada" },
        { "EnMantenimiento", "En mantenimiento" },
        { "DadaDeBaja", "Dada de baja" }
    };

    public static string Categoria(string valor)
    {
        if (Categorias.ContainsKey(valor))
        {
            return Categorias[valor];
        }

        return valor;
    }

    public static string EstadoDeUnidad(string valor)
    {
        if (EstadosDeUnidad.ContainsKey(valor))
        {
            return EstadosDeUnidad[valor];
        }

        return valor;
    }

    public static readonly IReadOnlyDictionary<string, string> AccionesAuditadas = new Dictionary<string, string>
    {
        { "RegistroDeUsuario", "Registro de usuario" },
        { "AltaDeUsuario", "Alta de usuario" },
        { "CambioDeTipoDeUsuario", "Cambio de tipo de usuario" },
        { "DesactivacionDeUsuario", "Desactivación de usuario" },
        { "ReactivacionDeUsuario", "Reactivación de usuario" },
        { "SolicitudDePrestamo", "Solicitud de préstamo" },
        { "AprobacionDePrestamo", "Aprobación de préstamo" },
        { "IntentoDeAprobacion", "Intento de aprobación" },
        { "RechazoDePrestamo", "Rechazo de préstamo" },
        { "ActivacionDePrestamo", "Activación de préstamo" },
        { "DevolucionDePrestamo", "Devolución de préstamo" }
    };

    public static string AccionAuditada(string valor)
    {
        if (AccionesAuditadas.ContainsKey(valor))
        {
            return AccionesAuditadas[valor];
        }

        return valor;
    }
}
