using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios;

public class ServicioAuditoria
{
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

        FiltroAuditoria filtroDelRepositorio = new FiltroAuditoria
        {
            Desde = filtro.Desde?.ToDateTime(TimeOnly.MinValue),
            Hasta = filtro.Hasta?.AddDays(1).ToDateTime(TimeOnly.MinValue),
            DniUsuario = filtro.DniUsuario
        };

        return _registros.ObtenerRegistros(filtroDelRepositorio).Select(CrearDto).ToList();
    }

    public bool TieneCriterios(FiltroAuditoriaDto filtro)
    {
        throw new NotImplementedException();
    }

    private static RegistroAuditoriaDto CrearDto(RegistroAuditoria registro)
    {
        return new RegistroAuditoriaDto { Detalle = registro.Detalle };
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
