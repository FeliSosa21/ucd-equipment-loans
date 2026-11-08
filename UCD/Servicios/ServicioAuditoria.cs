using System.Globalization;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioAuditoria
{
    private const string FormatoIso8601 = "yyyy-MM-ddTHH:mm:ss";

    private readonly RepositorioAuditoria _registros;
    private readonly RepositorioUsuario _usuarios;

    public ServicioAuditoria(RepositorioAuditoria registros, RepositorioUsuario usuarios)
    {
        _registros = registros;
        _usuarios = usuarios;
    }

    public List<RegistroAuditoriaDto> ListarRegistros(FiltroAuditoriaDto filtro, string dniResponsable)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ValidarQueSeaAdministradorActivo(dniResponsable);
        return _registros.ObtenerRegistros(CrearFiltro(filtro)).Select(CrearDto).ToList();
    }

    public bool TieneCriterios(FiltroAuditoriaDto filtro)
    {
        return CrearFiltro(filtro).TieneCriterios();
    }

    // Las fechas son dias completos: Hasta pasa como el inicio del dia siguiente porque el filtro lo excluye.
    private static FiltroAuditoria CrearFiltro(FiltroAuditoriaDto filtro)
    {
        return new FiltroAuditoria
        {
            Desde = filtro.Desde?.ToDateTime(TimeOnly.MinValue),
            Hasta = filtro.Hasta?.AddDays(1).ToDateTime(TimeOnly.MinValue),
            DniUsuario = string.IsNullOrWhiteSpace(filtro.DniUsuario) ? null : filtro.DniUsuario.Trim()
        };
    }

    private static RegistroAuditoriaDto CrearDto(RegistroAuditoria registro)
    {
        return new RegistroAuditoriaDto
        {
            FechaHora = registro.FechaHora.ToString(FormatoIso8601, CultureInfo.InvariantCulture),
            Detalle = registro.Detalle
        };
    }

    private void ValidarQueSeaAdministradorActivo(string dniResponsable)
    {
        Usuario? responsable = _usuarios.ObtenerUsuarioPorDni(dniResponsable);

        if (responsable is null || !responsable.EstaActivo() || !responsable.TienePermiso(Permiso.GestionarUsuarios))
        {
            throw new ServicioException("Solo un administrador activo puede consultar el log de auditoria");
        }
    }
}
