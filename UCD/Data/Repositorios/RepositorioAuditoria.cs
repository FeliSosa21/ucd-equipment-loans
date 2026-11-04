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
        _memoria.RegistrosAuditoria.Add(registro);
    }

    public IReadOnlyList<RegistroAuditoria> ObtenerPaginaDeRegistros(FiltroAuditoria filtro, int pagina, int tamanoPagina)
    {
        return _memoria.RegistrosAuditoria
            .AsEnumerable()
            .Reverse()
            .Where(registro => filtro.Desde is null || registro.FechaHora >= filtro.Desde)
            .Where(registro => filtro.Hasta is null || registro.FechaHora < filtro.Hasta)
            .OrderByDescending(registro => registro.FechaHora)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();
    }
}
