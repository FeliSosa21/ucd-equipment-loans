using Dominio;

namespace Data.Repositorios;

public class RepositorioAuditoria
{
    private readonly Memoria _memoria;

    public RepositorioAuditoria(Memoria memoria)
    {
        _memoria = memoria;
    }

    public void AgregarRegistro(RegistroAuditoria registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        _memoria.RegistrosAuditoria.Add(registro);
    }

    public IReadOnlyList<RegistroAuditoria> ObtenerPaginaDeRegistros(FiltroAuditoria filtro, int pagina, int tamanoPagina)
    {
        return Filtrar(filtro)
            .Reverse()
            .OrderByDescending(registro => registro.FechaHora)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();
    }

    public int ContarRegistros(FiltroAuditoria filtro)
    {
        return Filtrar(filtro).Count();
    }

    public IReadOnlyList<RegistroAuditoria> ObtenerRegistros(FiltroAuditoria filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        if (!filtro.TieneCriterios())
        {
            return new List<RegistroAuditoria>();
        }

        return Filtrar(filtro)
            .Reverse()
            .OrderByDescending(registro => registro.FechaHora)
            .ToList();
    }

    private IEnumerable<RegistroAuditoria> Filtrar(FiltroAuditoria filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        return _memoria.RegistrosAuditoria
            .Where(registro => filtro.Desde is null || registro.FechaHora >= filtro.Desde)
            .Where(registro => filtro.Hasta is null || registro.FechaHora < filtro.Hasta)
            .Where(registro => filtro.DniUsuario is null || registro.Responsable?.Dni == filtro.DniUsuario);
    }
}
