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

    [TestMethod]
    [DataRow(Permiso.ConsultarCatalogo)]
    [DataRow(Permiso.GestionarUnidades)]
    [DataRow(Permiso.RealizarPrestamo)]
    [DataRow(Permiso.RegistrarDevolucion)]
    public void TienePermiso_FuncionarioCEYPermisosPropios_DevuelveTrue(Permiso permiso)
    {
        var tipo = TipoUsuario.FuncionarioCE;

        var resultado = tipo.TienePermiso(permiso);
        
        Assert.IsTrue(resultado);
    }

    [TestMethod]
    [DataRow(Permiso.ConsultarDisponibilidad)]
    [DataRow(Permiso.SolicitarPrestamo)]
    [DataRow(Permiso.ConsultarMisSolicitudes)]
    [DataRow(Permiso.ConsultarHistorico)]
    public void TienePermiso_FuncionarioCEYPermisosAjenos_DevuelveFalse(Permiso permiso)
    {
        var tipo = TipoUsuario.FuncionarioCE;

        var resultado = tipo.TienePermiso(permiso);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    [DataRow(Permiso.ConsultarHistorico)]
    [DataRow(Permiso.GestionarUnidades)]
    [DataRow(Permiso.RealizarPrestamo)]
    [DataRow(Permiso.RegistrarDevolucion)]
    public void TienePermiso_EstudianteYPermisosAjenos_DevuelveFalse(Permiso permiso)
    {
        var tipo = TipoUsuario.Estudiante;

        var resultado = tipo.TienePermiso(permiso);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    [DataRow(Permiso.GestionarUnidades)]
    [DataRow(Permiso.RealizarPrestamo)]
    [DataRow(Permiso.RegistrarDevolucion)]
    public void TienePermiso_DocenteYPermisosAjenos_DevuelveFalse(Permiso permiso)
    {
        var tipo = TipoUsuario.Docente;
        
        var resultado = tipo.TienePermiso(permiso);
        
        Assert.IsFalse(resultado);
    }

    [TestMethod]
    [DataRow(Permiso.ConsultarCatalogo)]
    [DataRow(Permiso.ConsultarHistorico)]
    [DataRow(Permiso.GestionarCatalogo)]
    [DataRow(Permiso.GestionarUnidades)]
    [DataRow(Permiso.EvaluarSolicitud)]
    [DataRow(Permiso.RegistrarDevolucion)]
    [DataRow(Permiso.ConsultarPrestamos)]
    public void TienePermiso_SupervisorYPermisosPropios_DevuelveTrue(Permiso permiso)
    {
        var tipo = TipoUsuario.Supervisor;
        
        var resultado = tipo.TienePermiso(permiso);
        
        Assert.IsTrue(resultado);
    }
    

}