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

    private readonly RepositorioPrestamoSql _prestamos;
    private readonly RepositorioDevolucionSql _devoluciones;
    private readonly RepositorioUsuarioSql _usuarios;

    public ServicioExportacion(RepositorioPrestamoSql prestamos, RepositorioDevolucionSql devoluciones,
        RepositorioCatalogoSql catalogos, RepositorioUsuarioSql usuarios, IEnumerable<IExportadorDeTabla> exportadores)
    {
        _prestamos = prestamos;
        _devoluciones = devoluciones;
        _usuarios = usuarios;
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
