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
            .OrderByDescending(registro => registro.FechaHora)
            .ToList();
    }
}
