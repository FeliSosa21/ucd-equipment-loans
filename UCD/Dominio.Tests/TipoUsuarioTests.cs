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

    [TestMethod]
    [DataRow(Permiso.ConsultarCatalogo)]
    [DataRow(Permiso.ConsultarDisponibilidad)]
    [DataRow(Permiso.SolicitarPrestamo)]
    [DataRow(Permiso.ConsultarMisSolicitudes)]
    public void TienePermiso_EstudianteYPermisosPropios_DevuelveTrue(Permiso permiso)
    {
        var tipo = TipoUsuario.Estudiante;  
        
        var resultado = tipo.TienePermiso(permiso);
        
        Assert.IsTrue(resultado);
    }
    [TestMethod]
    [DataRow(Permiso.ConsultarCatalogo)]
    [DataRow(Permiso.ConsultarDisponibilidad)]
    [DataRow(Permiso.SolicitarPrestamo)]
    [DataRow(Permiso.ConsultarMisSolicitudes)]
    [DataRow(Permiso.ConsultarHistorico)]
    public void TienePermiso_DocenteYPermisosPropios_DevuelveTrue(Permiso permiso)
    {
        var tipo = TipoUsuario.Docente;

        var resultado = tipo.TienePermiso(permiso);
        
        Assert.IsTrue(resultado);
    }
    

}