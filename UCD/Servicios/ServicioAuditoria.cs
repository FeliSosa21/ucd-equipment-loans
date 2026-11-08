using Data.Repositorios;
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
        throw new NotImplementedException();
    }
}
