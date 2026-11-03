using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public sealed class RepositorioTipoUsuarioTests
{
    [TestMethod]
    public void ObtenerTipoPorNombre_TipoAgregado_LoDevuelve()
    {
        RepositorioTipoUsuario repositorio = new RepositorioTipoUsuario(new Memoria());
        TipoUsuario docente = TipoUsuario.Docente();
        repositorio.AgregarTipo(docente);

        TipoUsuario? resultado = repositorio.ObtenerTipoPorNombre(docente.Nombre);

        Assert.AreSame(docente, resultado);
    }
}
