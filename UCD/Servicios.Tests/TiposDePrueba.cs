using Data;
using Data.Repositorios;
using Dominio;

namespace Servicios.Tests;

internal static class TiposDePrueba
{
    public static void GuardarEn(SqlContext contexto)
    {
        RepositorioTipoUsuarioSql repositorioTipos = new RepositorioTipoUsuarioSql(contexto);
        foreach (TipoUsuario tipo in TipoUsuario.Todos())
        {
            repositorioTipos.AgregarTipo(tipo);
        }
    }
}
