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
        RegistroDeAccion registro = RegistroDeAccion.DeUsuario(responsable, _reloj.GetLocalNow().DateTime);
        _repositorio.AgregarRegistro(new RegistroAuditoria(registro, accion, detalle));
    }
}
