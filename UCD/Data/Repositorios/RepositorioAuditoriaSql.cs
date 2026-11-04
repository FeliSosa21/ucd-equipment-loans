using Dominio;
using Microsoft.EntityFrameworkCore;
using static Data.Configuraciones.NombresDeMapeo;

namespace Data.Repositorios;

public class RepositorioAuditoriaSql
{
    private readonly SqlContext _contexto;

    public RepositorioAuditoriaSql(SqlContext contexto)
    {
        _contexto = contexto;
    }

    public void AgregarRegistro(RegistroAuditoria registro)
    {
        _contexto.RegistrosAuditoria.Add(registro);
        _contexto.SaveChanges();
    }

    public IReadOnlyList<RegistroAuditoria> ObtenerPaginaDeRegistros(FiltroAuditoria filtro, int pagina, int tamanoPagina)
    {
        return _contexto.RegistrosAuditoria
            .OrderByDescending(registro => EF.Property<RegistroDeAccion>(registro, CampoRegistro).FechaHora)
            .ThenByDescending(registro => registro.Id)
            .ToList();
    }
}
