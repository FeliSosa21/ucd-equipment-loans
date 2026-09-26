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

    [TestMethod]

    public void TienePermiso_EstudianteConsultarCatalogo_DevuelveTrue()
    {
        var usuario = TipoUsuario.Estudiante;
        
        var resultado =  usuario.TienePermiso(Permiso.ConsultarCatalogo);

        Assert.IsTrue(resultado);
    }

    [TestMethod]

    public void Estudiante_PermisosRepetidos_DeveuelveLaMismaInstancia()
    {
        var primero = TipoUsuario.Estudiante;
        var segundo = TipoUsuario.Estudiante;

        Assert.AreSame(primero, segundo);
    }

}