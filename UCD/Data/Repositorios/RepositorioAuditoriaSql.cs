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
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();
    }

    public IReadOnlyList<RegistroAuditoria> ObtenerRegistros(FiltroAuditoria filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        if (!filtro.TieneCriterios())
        {
            return new List<RegistroAuditoria>();
        }

        return _contexto.RegistrosAuditoria
            .Where(registro => filtro.Desde == null
                               || EF.Property<RegistroDeAccion>(registro, CampoRegistro).FechaHora >= filtro.Desde)
            .OrderByDescending(registro => EF.Property<RegistroDeAccion>(registro, CampoRegistro).FechaHora)
            .ToList();
    }
}
