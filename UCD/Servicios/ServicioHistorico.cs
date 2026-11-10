using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioHistorico
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para consultar el historico";
    private const string MensajeDesdePosteriorAHasta = "La fecha desde no puede ser posterior a la fecha hasta";
    private const string ResponsableSistema = "Sistema";

    private readonly RepositorioCatalogoSql _catalogos;
    private readonly RepositorioUsuarioSql _usuarios;
    private readonly RepositorioMovimientoSql _movimientos;

    public ServicioHistorico(RepositorioCatalogoSql catalogos, RepositorioUsuarioSql usuarios,
        RepositorioMovimientoSql movimientos)
    {
        _catalogos = catalogos;
        _usuarios = usuarios;
        _movimientos = movimientos;
    }

    public List<MovimientoDto> ListarMovimientos(int idCatalogo, FiltroMovimientosDto filtro, string dniUsuario)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        OperacionesComunes.ValidarResponsable(_usuarios.ObtenerUsuarioPorDni(dniUsuario), Permiso.ConsultarHistorico,
            MensajeSinPermiso);
        Catalogo catalogo = OperacionesComunes.ObtenerCatalogoActivo(_catalogos, idCatalogo);
        ValidarRangoDeFechas(filtro);

        return _movimientos.ObtenerMovimientos(CrearFiltroDelRepositorio(catalogo.Id, filtro))
            .Select(CrearMovimientoDto)
            .ToList();
    }

    private static void ValidarRangoDeFechas(FiltroMovimientosDto filtro)
    {
        if (filtro.Desde > filtro.Hasta)
        {
            throw new ServicioException(nameof(FiltroMovimientosDto.Desde), MensajeDesdePosteriorAHasta);
        }
    }

    private static FiltroPorCatalogoYFechas CrearFiltroDelRepositorio(int idCatalogo, FiltroMovimientosDto filtro)
    {
        return new FiltroPorCatalogoYFechas
        {
            IdCatalogo = idCatalogo,
            Desde = filtro.Desde,
            Hasta = InicioDelDiaSiguiente(filtro.Hasta)
        };
    }

    // Hasta es un dia completo: pasa como el inicio del dia siguiente porque el repositorio lo excluye (B11).
    // La ultima fecha posible no tiene dia siguiente: se toma como sin limite, igual que en ServicioAuditoria.
    private static DateTime? InicioDelDiaSiguiente(DateTime? hasta)
    {
        if (hasta is null || hasta.Value.Date == DateTime.MaxValue.Date)
        {
            return null;
        }

        return hasta.Value.Date.AddDays(1);
    }

    // Temporal, hasta que la pagina filtre por catalogo: no recibe quien consulta (el permiso lo controla la pagina).
    // El repositorio no devuelve nada sin criterios, asi que Desde = MinValue pide todo lo registrado.
    public List<MovimientoDto> ListarMovimientos()
    {
        FiltroPorCatalogoYFechas desdeSiempre = new FiltroPorCatalogoYFechas { Desde = DateTime.MinValue };

        return _movimientos.ObtenerMovimientos(desdeSiempre)
            .Select(CrearMovimientoDto)
            .ToList();
    }

    private static MovimientoDto CrearMovimientoDto(Movimiento movimiento)
    {
        return new MovimientoDto
        {
            FechaHora = movimiento.FechaHora,
            Tipo = movimiento.Tipo.ToString(),
            Solicitante = movimiento.Solicitante.NombreCompleto,
            Evaluador = movimiento.Evaluador?.NombreCompleto,
            Responsable = movimiento.Responsable?.NombreCompleto ?? ResponsableSistema,
            Unidad = movimiento.Unidad?.IdInstitucional
        };
    }
}
