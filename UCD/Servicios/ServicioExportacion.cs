using System.Globalization;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

// Exporta los prestamos (por fecha de solicitud) y las devoluciones (por fecha de registro) de un rango de dias,
// de todos los catalogos, en el formato que declare alguno de los exportadores registrados.
//
// Limitacion conocida: el CSV no neutraliza las celdas que empiezan con =, +, - o @ (inyeccion de formulas al
// abrirlo en una planilla). El XLSX guarda todo como texto y no la tiene; anteponer un apostrofo solo en el CSV
// haria que los dos archivos dejen de tener la misma informacion.
public class ServicioExportacion
{
    private const string MensajeSinPermiso = "No tiene permiso para exportar prestamos y devoluciones";
    private const string MensajeFaltaDesde = "Elija la fecha desde";
    private const string MensajeFaltaHasta = "Elija la fecha hasta";
    private const string MensajeRangoInvertido = "La fecha desde no puede ser posterior a la fecha hasta";
    private const string MensajeFormatoDesconocido = "El formato elegido no esta disponible";
    private const string FormatoDeFechaEnElNombre = "yyyy-MM-dd";

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

    public List<string> FormatosDisponibles()
    {
        return _exportadores.Select(exportador => exportador.Formato).ToList();
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

    public List<ArchivoExportadoDto> Exportar(PedidoExportacionDto pedido, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        ValidarResponsable(dniResponsable);
        ValidarRango(pedido.Rango);
        IExportadorDeTabla exportador = ObtenerExportador(pedido.Formato);
        FiltroPorCatalogoYFechas filtro = CrearFiltro(pedido.Rango);
        Dictionary<int, string> catalogoPorTipo = ConstructorDeTablasExportables.CatalogoPorTipo(
            _catalogos.ObtenerCatalogosParaExportar());
        TablaExportableDto prestamos = ConstructorDeTablasExportables.TablaDePrestamos(
            _prestamos.ObtenerPrestamos(filtro), catalogoPorTipo);
        TablaExportableDto devoluciones = ConstructorDeTablasExportables.TablaDeDevoluciones(
            _devoluciones.ObtenerDevoluciones(filtro), catalogoPorTipo);
        string sufijoDelNombre = CrearSufijoDelNombre(pedido.Rango);

        return new List<ArchivoExportadoDto>
        {
            CrearArchivo("prestamos" + sufijoDelNombre, prestamos, exportador),
            CrearArchivo("devoluciones" + sufijoDelNombre, devoluciones, exportador)
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
}
