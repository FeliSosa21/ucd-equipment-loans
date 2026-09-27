namespace Dominio.Tests;

[TestClass]

public sealed class TipoUsuarioTests
{
    [TestMethod]
    public void TienePermiso_SinPermisosYConsultarCatalogo_DevuelveFalse()
    {
        var tipo = TipoUsuario.SinPermisos;

        var resultado = tipo.TienePermiso(Permiso.ConsultarCatalogo);

        Assert.IsFalse(resultado);

    }
    [TestMethod]
    public void TienePermiso_EstudianteYConsultarCatalogo_DevuelveTrue()
    {
        var tipo = TipoUsuario.Estudiante;
        
        var resultado =  tipo.TienePermiso(Permiso.ConsultarCatalogo);

        Assert.IsTrue(resultado);
    }
    [TestMethod]
    public void Estudiante_AccesosRepetidos_DevuelveLaMismaInstancia()
    {
        var primero = TipoUsuario.Estudiante;
        var segundo = TipoUsuario.Estudiante;

        Assert.AreSame(primero, segundo);
    }
    [TestMethod]
    public void TienePermiso_EstudianteYConsultarHistorico_DevuelveFalse()
    {
        var tipo = TipoUsuario.Estudiante;
        
        var resultado = tipo.TienePermiso(Permiso.ConsultarHistorico);
        Assert.IsFalse(resultado);  
        
    }



}