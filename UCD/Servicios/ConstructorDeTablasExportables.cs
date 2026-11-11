using System.Globalization;
using Dominio;
using DTOs;

namespace Servicios;

// Convierte prestamos y devoluciones en tablas de texto. No consulta nada ni conoce los formatos de archivo:
// esa division deja al servicio con los permisos y el flujo, y a los exportadores con los bytes.
internal static class ConstructorDeTablasExportables
{
    // ISO-8601, sin zona: las fechas del sistema son hora local.
    private const string FormatoDeFechaHora = "yyyy-MM-ddTHH:mm:ss";
    private const string FormatoDeFecha = "yyyy-MM-dd";
    private const string SeparadorDeLista = "; ";

    private static readonly Dictionary<CondicionDevolucion, string> TextoDeLaCondicion =
        new Dictionary<CondicionDevolucion, string>
        {
            { CondicionDevolucion.SinObservaciones, "Sin observaciones" },
            { CondicionDevolucion.ConFallas, "Con fallas" },
            { CondicionDevolucion.Daniada, "Dañada" }
        };

    // Id del tipo -> nombre de su catalogo: el prestamo conoce su tipo, no el catalogo del tipo.
    public static Dictionary<int, string> CatalogoPorTipo(IEnumerable<Catalogo> catalogos)
    {
        Dictionary<int, string> catalogoPorTipo = new Dictionary<int, string>();
        foreach (Catalogo catalogo in catalogos)
        {
            foreach (TipoEquipamiento tipo in catalogo.Tipos)
            {
                catalogoPorTipo[tipo.Id] = catalogo.Nombre;
            }
        }

        return catalogoPorTipo;
    }

    public static TablaExportableDto TablaDePrestamos(IEnumerable<Prestamo> prestamos,
        IReadOnlyDictionary<int, string> catalogoPorTipo)
    {
        TablaExportableDto tabla = new TablaExportableDto
        {
            Encabezados = new List<string>
            {
                "Catalogo", "Fecha de solicitud", "Solicitante", "Tipo de equipamiento", "Cantidad",
                "Unidades asignadas", "Estado", "Evaluador", "Fecha esperada de devolucion"
            }
        };

        foreach (Prestamo prestamo in prestamos)
        {
            tabla.Filas.Add(FilaDePrestamo(prestamo, catalogoPorTipo[prestamo.TipoEquipamiento.Id]));
        }

        return tabla;
    }

    public static TablaExportableDto TablaDeDevoluciones(IEnumerable<Devolucion> devoluciones,
        IReadOnlyDictionary<int, string> catalogoPorTipo)
    {
        TablaExportableDto tabla = new TablaExportableDto
        {
            Encabezados = new List<string>
            {
                "Catalogo", "Fecha de registro", "Solicitante", "Tipo de equipamiento", "Unidades devueltas",
                "Registrada por"
            }
        };

        foreach (Devolucion devolucion in devoluciones)
        {
            tabla.Filas.Add(FilaDeDevolucion(devolucion, catalogoPorTipo[devolucion.Prestamo.TipoEquipamiento.Id]));
        }

        return tabla;
    }

    private static List<string> FilaDePrestamo(Prestamo prestamo, string catalogo)
    {
        return new List<string>
        {
            catalogo,
            prestamo.FechaHoraPrestamo.ToString(FormatoDeFechaHora, CultureInfo.InvariantCulture),
            prestamo.Solicitante.NombreCompletoConDni,
            prestamo.TipoEquipamiento.Nombre,
            prestamo.CantidadSolicitada.ToString(CultureInfo.InvariantCulture),
            string.Join(SeparadorDeLista, prestamo.UnidadesAsignadas.Select(unidad => unidad.IdInstitucional)),
            prestamo.Estado.ToString(),
            prestamo.Evaluador?.NombreCompletoConDni ?? string.Empty,
            prestamo.FechaEsperadaDevolucion.ToString(FormatoDeFecha, CultureInfo.InvariantCulture)
        };
    }

    private static List<string> FilaDeDevolucion(Devolucion devolucion, string catalogo)
    {
        return new List<string>
        {
            catalogo,
            devolucion.FechaHoraDevolucion.ToString(FormatoDeFechaHora, CultureInfo.InvariantCulture),
            devolucion.Prestamo.Solicitante.NombreCompletoConDni,
            devolucion.Prestamo.TipoEquipamiento.Nombre,
            string.Join(SeparadorDeLista, devolucion.Unidades.Select(DescribirUnidadDevuelta)),
            devolucion.RegistradaPor.NombreCompletoConDni
        };
    }

    private static string DescribirUnidadDevuelta(UnidadDevuelta unidadDevuelta)
    {
        return $"{unidadDevuelta.Unidad.IdInstitucional} ({TextoDeLaCondicion[unidadDevuelta.Condicion]})";
    }
}
