using Data;
using Data.Repositorios;
using Dominio;

namespace Servicios.Tests;

// Repositorio de tipos con los seis predefinidos cargados, como queda la app despues de la carga inicial.
internal static class TiposDePrueba
{
    public static RepositorioTipoUsuario Cargados(Memoria memoria)
    {
        RepositorioTipoUsuario repositorioTipos = new RepositorioTipoUsuario(memoria);
        foreach (TipoUsuario tipo in TipoUsuario.Todos())
        {
            repositorioTipos.AgregarTipo(tipo);
        }

        return repositorioTipos;
    }
}
