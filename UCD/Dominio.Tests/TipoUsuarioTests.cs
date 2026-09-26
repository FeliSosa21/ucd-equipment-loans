namespace Dominio.Tests;

[TestClass]

public sealed class TipoUsuarioTests
{
    [TestMethod]

    public void TienePermiso_SinPermisosYConsultarCatalogo_DevuelveFalse()
    {
        var usuario = TipoUsuario.SinPermisos;

        var resultado = usuario.TienePermiso(Permiso.ConsultarCatalogo);

        Assert.IsFalse(resultado);

    }

}