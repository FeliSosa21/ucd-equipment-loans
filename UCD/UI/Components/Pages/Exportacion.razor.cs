using System.Globalization;
using DTOs;
using Microsoft.JSInterop;
using Servicios;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class Exportacion
{
    private const string CampoDesde = nameof(RangoDeExportacionDto.Desde);
    private const string CampoHasta = nameof(RangoDeExportacionDto.Hasta);
    private const string FormatoDelCampoFecha = "yyyy-MM-dd";
    private const string FuncionDeDescarga = "descargarArchivo";
    private const int DiasPorDefecto = 30;

    private List<string> _formatos = new List<string>();
    private DateOnly? _desde;
    private DateOnly? _hasta;
    private CantidadesAExportarDto? _cantidades;
    private string? _exito;
    private bool _descargando;
    private readonly ErroresDeFormulario _errores = new ErroresDeFormulario(CampoDesde, CampoHasta);


    // Los calendarios no ofrecen rangos al reves; igual el servicio valida el rango.
    private string? MaximoDeDesde => _hasta?.ToString(FormatoDelCampoFecha, CultureInfo.InvariantCulture);
    private string? MinimoDeHasta => _desde?.ToString(FormatoDelCampoFecha, CultureInfo.InvariantCulture);

    // Sin cantidades no hay rango valido; durante una descarga no se acepta otra.
    private bool PuedeDescargar => _cantidades is not null && !_descargando;

    private bool NoHayRegistros => _cantidades is { Prestamos: 0, Devoluciones: 0 };

    private string TextoDeCantidades => $"{Contar(_cantidades!.Prestamos, "préstamo", "préstamos")} y " +
                                        Contar(_cantidades.Devoluciones, "devolución", "devoluciones");

    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.ConsultarPrestamos))
        {
            return;
        }

        _formatos = ServicioExportacion.FormatosDisponibles();
        _hasta = DateOnly.FromDateTime(Reloj.GetLocalNow().DateTime);
        _desde = _hasta.Value.AddDays(-DiasPorDefecto);
        ActualizarCantidades();
    }

    // Si el rango no es valido o quien exporta perdio el permiso, el servicio lo niega y se muestra el motivo.
    private void ActualizarCantidades()
    {
        _errores.Limpiar();
        _exito = null;

        try
        {
            _cantidades = ServicioExportacion.ContarRegistros(CrearRango(), DniUsuario);
        }
        catch (ServicioException ex)
        {
            _cantidades = null;
            _errores.Agregar(ex);
        }
    }

    private async Task Descargar(string formato)
    {
        // Un doble clic manda dos envios: el segundo llega con la descarga en curso.
        if (!PuedeDescargar)
        {
            return;
        }

        _errores.Limpiar();
        _exito = null;
        _descargando = true;
        // Se muestra "Preparando..." y se deshabilitan los botones antes del trabajo del servicio.
        StateHasChanged();
        await Task.Yield();

        try
        {
            List<ArchivoExportadoDto> archivos = ServicioExportacion.Exportar(CrearPedido(formato), DniUsuario);
            foreach (ArchivoExportadoDto archivo in archivos)
            {
                await Entregar(archivo);
            }

            _exito = $"Se descargaron {archivos.Count} archivos: {string.Join(" y ", archivos.Select(a => a.Nombre))}.";
        }
        catch (ServicioException ex)
        {
            _errores.Agregar(ex);
        }
        finally
        {
            _descargando = false;
        }
    }

    // El archivo viaja al navegador como flujo: no se pasa por un string base64 ni por una URL del servidor.
    private async Task Entregar(ArchivoExportadoDto archivo)
    {
        using MemoryStream flujo = new MemoryStream(archivo.Contenido);
        using DotNetStreamReference referencia = new DotNetStreamReference(flujo);
        await JS.InvokeVoidAsync(FuncionDeDescarga, archivo.Nombre, archivo.TipoContenido, referencia);
    }

    private RangoDeExportacionDto CrearRango()
    {
        return new RangoDeExportacionDto { Desde = _desde, Hasta = _hasta };
    }

    private PedidoExportacionDto CrearPedido(string formato)
    {
        return new PedidoExportacionDto { Rango = CrearRango(), Formato = formato };
    }

    private static string Contar(int cantidad, string singular, string plural)
    {
        return $"{cantidad} {(cantidad == 1 ? singular : plural)}";
    }
}
