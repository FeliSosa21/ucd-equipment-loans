using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioHistorico
{
    private const string MensajeSinPermiso = "El usuario no tiene permiso para consultar el historico";

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
        OperacionesComunes.ValidarResponsable(_usuarios.ObtenerUsuarioPorDni(dniUsuario), Permiso.ConsultarHistorico,
            MensajeSinPermiso);

        return new List<MovimientoDto>();
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
