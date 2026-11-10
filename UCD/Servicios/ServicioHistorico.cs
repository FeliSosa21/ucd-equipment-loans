using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioHistorico
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para consultar el historico";
    private const string MensajeDesdePosteriorAHasta = "La fecha desde no puede ser posterior a la fecha hasta";

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
        FiltroPorCatalogoYFechas filtroDelRepositorio = new FiltroPorCatalogoYFechas
        {
            IdCatalogo = catalogo.Id,
            Desde = filtro.Desde,
            Hasta = filtro.Hasta?.Date == DateTime.MaxValue.Date ? null : filtro.Hasta?.Date.AddDays(1)
        };

        return _movimientos.ObtenerMovimientos(filtroDelRepositorio)
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
            Responsable = movimiento.Responsable?.NombreCompleto ?? "Sistema",
            Unidad = movimiento.Unidad?.IdInstitucional
        };
    }
}
