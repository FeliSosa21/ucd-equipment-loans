using DTOs;
using Servicios;
using UI.Presentacion;
using UI.Sesion;

namespace UI.Components.Pages;

public partial class LogAuditoria
{
    private const int DiasHaciaAtrasPorDefecto = 7;
    private const string CampoDesde = nameof(FiltroAuditoriaDto.Desde);

    private List<RegistroAuditoriaDto> _registros = new List<RegistroAuditoriaDto>();
    private DateOnly? _desde;
    private DateOnly? _hasta;
    private string _dni = string.Empty;
    private bool _filtroConCriterios;
    private readonly ErroresDeFormulario _errores = new ErroresDeFormulario(CampoDesde);

    private bool HayError => _errores.Tiene(CampoDesde) || _errores.General is not null;

    private string TituloSinRegistros => _filtroConCriterios
        ? "No hay acciones registradas con esos filtros"
        : "Elegí un rango de fechas o un DNI del responsable";

    // Arranca mostrando la ultima semana: sin ningun filtro el servicio no devuelve registros.
    protected override void OnInitialized()
    {
        if (RedirigirSiNoTienePermiso(Permisos.GestionarUsuarios))
        {
            return;
        }

        _desde = DateOnly.FromDateTime(Reloj.GetLocalNow().DateTime).AddDays(-DiasHaciaAtrasPorDefecto);
        Recargar();
    }

    // Si quien mira el log dejo de ser administrador, el servicio lo niega y se muestra el motivo.
    private void Recargar()
    {
        _errores.Limpiar();
        FiltroAuditoriaDto filtro = new FiltroAuditoriaDto { Desde = _desde, Hasta = _hasta, DniUsuario = _dni };

        try
        {
            _filtroConCriterios = ServicioAuditoria.TieneCriterios(filtro);
            _registros = ServicioAuditoria.ListarRegistros(filtro, DniUsuario);
        }
        catch (ServicioException ex)
        {
            _registros = new List<RegistroAuditoriaDto>();
            _errores.Agregar(ex);
        }
    }
}
