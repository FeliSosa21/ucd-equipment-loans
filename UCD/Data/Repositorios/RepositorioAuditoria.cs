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
    }

    public IReadOnlyList<RegistroAuditoria> ObtenerPaginaDeRegistros(FiltroAuditoria filtro, int pagina, int tamanoPagina)
    {
        return new List<RegistroAuditoria>();
    }
}
