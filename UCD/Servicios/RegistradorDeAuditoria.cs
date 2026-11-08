using Data.Repositorios;
using Dominio;

namespace Servicios;

public class RegistradorDeAuditoria
{
    private readonly RepositorioAuditoria _repositorio;
    private readonly TimeProvider _reloj;

    public RegistradorDeAuditoria(RepositorioAuditoria repositorio, TimeProvider reloj)
    {
        _repositorio = repositorio;
        _reloj = reloj;
    }

    public void RegistrarDeUsuario(AccionAuditada accion, Usuario responsable, string detalle)
    {
        throw new NotImplementedException();
    }
}
