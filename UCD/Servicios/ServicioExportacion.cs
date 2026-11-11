using System.Globalization;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioExportacion
{
    private const string MensajeSinPermiso = "No tiene permiso para exportar prestamos y devoluciones";
    private const string MensajeFaltaDesde = "Elija la fecha desde";
    private const string MensajeFaltaHasta = "Elija la fecha hasta";
    private const string MensajeRangoInvertido = "La fecha desde no puede ser posterior a la fecha hasta";
    private const string MensajeFormatoDesconocido = "El formato elegido no esta disponible";
    private const string FormatoDeFechaEnElNombre = "yyyy-MM-dd";
    private const string FormatoDeFechaHora = "yyyy-MM-ddTHH:mm:ss";
    private const string SeparadorDeLista = "; ";

    private readonly RepositorioPrestamoSql _prestamos;
    private readonly RepositorioDevolucionSql _devoluciones;
    private readonly RepositorioCatalogoSql _catalogos;
    private readonly RepositorioUsuarioSql _usuarios;
    private readonly List<IExportadorDeTabla> _exportadores;

    public ServicioExportacion(RepositorioPrestamoSql prestamos, RepositorioDevolucionSql devoluciones,
        RepositorioCatalogoSql catalogos, RepositorioUsuarioSql usuarios, IEnumerable<IExportadorDeTabla> exportadores)
    {
        _prestamos = prestamos;
        _devoluciones = devoluciones;
        _catalogos = catalogos;
        _usuarios = usuarios;
        _exportadores = exportadores.ToList();
    }

    public List<ArchivoExportadoDto> Exportar(PedidoExportacionDto pedido, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        ValidarResponsable(dniResponsable);
        ValidarRango(pedido.Rango);
        IExportadorDeTabla exportador = ObtenerExportador(pedido.Formato);
        string sufijoDelNombre = CrearSufijoDelNombre(pedido.Rango);
        FiltroPorCatalogoYFechas filtro = CrearFiltro(pedido.Rango);
        Dictionary<int, string> catalogoDelTipo = CrearCatalogoPorTipo();

        return new List<ArchivoExportadoDto>
        {
            CrearArchivo("prestamos" + sufijoDelNombre, CrearTablaDePrestamos(filtro, catalogoDelTipo), exportador),
            CrearArchivo("devoluciones" + sufijoDelNombre, new TablaExportableDto(), exportador)
        };
    }

    // Id del tipo -> nombre de su catalogo, de todos los catalogos (los eliminados tambien: sus prestamos se exportan).
    private Dictionary<int, string> CrearCatalogoPorTipo()
    {
        Dictionary<int, string> catalogoPorTipo = new Dictionary<int, string>();
        foreach (Catalogo catalogo in _catalogos.ObtenerCatalogosParaExportar())
        {
            foreach (TipoEquipamiento tipo in catalogo.Tipos)
            {
                catalogoPorTipo[tipo.Id] = catalogo.Nombre;
            }
        }

        return catalogoPorTipo;
    }

    private TablaExportableDto CrearTablaDePrestamos(FiltroPorCatalogoYFechas filtro,
        Dictionary<int, string> catalogoDelTipo)
    {
        TablaExportableDto tabla = new TablaExportableDto
        {
            Encabezados = new List<string>
            {
                "Catalogo", "Fecha de solicitud", "Solicitante", "Tipo de equipamiento", "Cantidad",
                "Unidades asignadas", "Estado", "Evaluador", "Fecha esperada de devolucion"
            }
        };

        foreach (Prestamo prestamo in _prestamos.ObtenerPrestamos(filtro))
        {
            tabla.Filas.Add(CrearFilaDePrestamo(prestamo, catalogoDelTipo[prestamo.TipoEquipamiento.Id]));
        }

        return tabla;
    }

    private static List<string> CrearFilaDePrestamo(Prestamo prestamo, string catalogo)
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
            prestamo.FechaEsperadaDevolucion.ToString(FormatoDeFechaEnElNombre, CultureInfo.InvariantCulture)
        };
    }

    // Cada exportador declara su formato: agregar uno nuevo no obliga a tocar este servicio.
    private IExportadorDeTabla ObtenerExportador(string? formato)
    {
        IExportadorDeTabla? exportador = _exportadores.FirstOrDefault(candidato =>
            string.Equals(candidato.Formato, formato, StringComparison.OrdinalIgnoreCase));

        if (exportador is null)
        {
            throw new ServicioException(nameof(PedidoExportacionDto.Formato), MensajeFormatoDesconocido);
        }

        return exportador;
    }

    private static string CrearSufijoDelNombre(RangoDeExportacionDto rango)
    {
        string desde = rango.Desde!.Value.ToString(FormatoDeFechaEnElNombre, CultureInfo.InvariantCulture);
        string hasta = rango.Hasta!.Value.ToString(FormatoDeFechaEnElNombre, CultureInfo.InvariantCulture);
        return $"_{desde}_{hasta}";
    }

    private static ArchivoExportadoDto CrearArchivo(string nombreBase, TablaExportableDto tabla,
        IExportadorDeTabla exportador)
    {
        return new ArchivoExportadoDto
        {
            Nombre = nombreBase + exportador.Extension,
            Contenido = exportador.Exportar(tabla),
            TipoContenido = exportador.TipoContenido
        };
    }

    public CantidadesAExportarDto ContarRegistros(RangoDeExportacionDto rango, string dniResponsable)
    {
        ValidarResponsable(dniResponsable);
        ValidarRango(rango);

        FiltroPorCatalogoYFechas filtro = CrearFiltro(rango);

        return new CantidadesAExportarDto
        {
            Prestamos = _prestamos.ObtenerPrestamos(filtro).Count,
            Devoluciones = _devoluciones.ObtenerDevoluciones(filtro).Count
        };
    }

    // Sin catalogo: se exportan todos juntos. Las fechas son dias completos: Hasta pasa como el inicio del dia
    // siguiente porque el repositorio lo excluye.
    private static FiltroPorCatalogoYFechas CrearFiltro(RangoDeExportacionDto rango)
    {
        return new FiltroPorCatalogoYFechas
        {
            IdCatalogo = null,
            Desde = rango.Desde!.Value.ToDateTime(TimeOnly.MinValue),
            Hasta = rango.Hasta!.Value.AddDays(1).ToDateTime(TimeOnly.MinValue)
        };
    }

    // Se valida en cada operacion: la pantalla pudo abrirse antes de que desactivaran al usuario.
    private void ValidarResponsable(string dniResponsable)
    {
        Usuario? responsable = _usuarios.ObtenerUsuarioPorDni(dniResponsable);
        OperacionesComunes.ValidarResponsable(responsable, Permiso.ConsultarPrestamos, MensajeSinPermiso);
    }

    private static void ValidarRango(RangoDeExportacionDto rango)
    {
        ArgumentNullException.ThrowIfNull(rango);

        if (rango.Desde is null)
        {
            throw new ServicioException(nameof(RangoDeExportacionDto.Desde), MensajeFaltaDesde);
        }

        if (rango.Hasta is null)
        {
            throw new ServicioException(nameof(RangoDeExportacionDto.Hasta), MensajeFaltaHasta);
        }

        if (rango.Desde > rango.Hasta)
        {
            throw new ServicioException(nameof(RangoDeExportacionDto.Desde), MensajeRangoInvertido);
        }
    }
}
