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
        return new List<RegistroAuditoriaDto>();
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
