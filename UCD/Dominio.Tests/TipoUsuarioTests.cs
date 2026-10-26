namespace Dominio.Tests;

[TestClass]
public sealed class TipoUsuarioTests
{
    private const string ImagenPng = "data:image/png;base64,iVBORw0KGgo=";

    [TestMethod]
    public void TienePermiso_SinPermisosYCualquierPermiso_DevuelveFalse()
    {
        var tipo = TipoUsuario.SinPermisos;

        foreach (var permiso in Enum.GetValues<Permiso>())
        {
            var resultado = tipo.TienePermiso(permiso);

            Assert.IsFalse(resultado, $"SinPermisos no deberia tener el permiso: {permiso}");
        }
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
    [DataRow(Permiso.GestionarCatalogo)]
    [DataRow(Permiso.EvaluarSolicitud)]
    [DataRow(Permiso.ConsultarPrestamos)]
    [DataRow(Permiso.GestionarUsuarios)]
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
    [DataRow(Permiso.GestionarCatalogo)]
    [DataRow(Permiso.EvaluarSolicitud)]
    [DataRow(Permiso.ConsultarPrestamos)]
    [DataRow(Permiso.GestionarUsuarios)]
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
    [DataRow(Permiso.GestionarCatalogo)]
    [DataRow(Permiso.EvaluarSolicitud)]
    [DataRow(Permiso.ConsultarPrestamos)]
    [DataRow(Permiso.GestionarUsuarios)]
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

    [TestMethod]
    [DataRow(Permiso.ConsultarDisponibilidad)]
    [DataRow(Permiso.SolicitarPrestamo)]
    [DataRow(Permiso.ConsultarMisSolicitudes)]
    [DataRow(Permiso.RealizarPrestamo)]
    [DataRow(Permiso.GestionarUsuarios)]
    public void TienePermiso_SupervisorYPermisosAjenos_DevuelveFalse(Permiso permiso)
    {
        var tipo = TipoUsuario.Supervisor;

        var resultado = tipo.TienePermiso(permiso);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void TienePermiso_AdministradorYCualquierPermiso_DevuelveTrue()
    {
        var tipo = TipoUsuario.Administrador;

        foreach (var permiso in Enum.GetValues<Permiso>())
        {
            var resultado = tipo.TienePermiso(permiso);

            Assert.IsTrue(resultado, $"El administrador deberia tener {permiso}");
        }
    }

    [TestMethod]
    public void Nombre_Administrador_DevuelveAdministrador()
    {
        var tipo = TipoUsuario.Administrador;

        var nombre = tipo.Nombre;

        Assert.AreEqual("Administrador", nombre);
    }

    [TestMethod]
    public void Nombre_Docente_DevuelveDocente()
    {
        var tipo = TipoUsuario.Docente;

        var nombre = tipo.Nombre;

        Assert.AreEqual("Docente", nombre);
    }

    [TestMethod]
    public void Nombre_Estudiante_DevuelveEstudiante()
    {
        var tipo = TipoUsuario.Estudiante;

        var nombre = tipo.Nombre;

        Assert.AreEqual("Estudiante", nombre);
    }

    [TestMethod]
    public void Nombre_FuncionarioCE_DevuelveFuncionarioDelCentroDeEquipamiento()
    {
        var tipo = TipoUsuario.FuncionarioCE;

        var nombre = tipo.Nombre;

        Assert.AreEqual("Funcionario del centro de equipamiento", nombre);
    }

    [TestMethod]
    public void Nombre_Supervisor_DevuelveSupervisor()
    {
        var tipo = TipoUsuario.Supervisor;

        var nombre = tipo.Nombre;

        Assert.AreEqual("Supervisor", nombre);
    }

    [TestMethod]
    public void Nombre_SinPermisos_DevuelveSinPermisos()
    {
        var tipo = TipoUsuario.SinPermisos;

        var nombre = tipo.Nombre;

        Assert.AreEqual("Sin permisos", nombre);
    }

    [TestMethod]
    public void Todos_SinArgumentos_DevuelveLosSeisTiposDeUsuario()
    {
        var esperados = new[]
        {
            TipoUsuario.SinPermisos,
            TipoUsuario.Estudiante,
            TipoUsuario.Docente,
            TipoUsuario.FuncionarioCE,
            TipoUsuario.Supervisor,
            TipoUsuario.Administrador
        };

        var todos = TipoUsuario.Todos().ToList();

        CollectionAssert.AreEquivalent(esperados, todos);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void PuedeAgregarPrestamo_EstudianteDebajoDelMaximo_DevuelveTrue(int prestamosVigentes)
    {
        var tipo = TipoUsuario.Estudiante;

        var resultado = tipo.PuedeAgregarPrestamo(prestamosVigentes);

        Assert.IsTrue(resultado);
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(3)]
    public void PuedeAgregarPrestamo_EstudianteEnElMaximoOMas_DevuelveFalse(int prestamosVigentes)
    {
        var tipo = TipoUsuario.Estudiante;

        var resultado = tipo.PuedeAgregarPrestamo(prestamosVigentes);
        
        Assert.IsFalse(resultado);
    }
    
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public void PuedeAgregarPrestamo_DocenteDebajoDelMaximo_DevuelveTrue(int prestamosVigentes)
    {
        var tipo = TipoUsuario.Docente;

        var resultado = tipo.PuedeAgregarPrestamo(prestamosVigentes);

        Assert.IsTrue(resultado);
    }

    [TestMethod]
    [DataRow(3)]
    [DataRow(4)]
    public void PuedeAgregarPrestamo_DocenteEnElMaximoOMas_DevuelveFalse(int prestamosVigentes)
    {
        var tipo = TipoUsuario.Docente;

        var resultado = tipo.PuedeAgregarPrestamo(prestamosVigentes);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1000)]
    public void PuedeAgregarPrestamo_AdministradorConCualquierCantidad_DevuelveTrue(int prestamosVigentes)
    {
        var tipo = TipoUsuario.Administrador;
        
        var resultado = tipo.PuedeAgregarPrestamo(prestamosVigentes);
        
        Assert.IsTrue(resultado);
    }
    
    [TestMethod]
    public void PuedeAgregarPrestamo_FuncionarioCESinPrestamos_DevuelveFalse()
    {
        var tipo = TipoUsuario.FuncionarioCE;

        var resultado = tipo.PuedeAgregarPrestamo(0);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void PuedeAgregarPrestamo_SupervisorSinPrestamos_DevuelveFalse()
    {
        var tipo = TipoUsuario.Supervisor;

        var resultado = tipo.PuedeAgregarPrestamo(0);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void PuedeAgregarPrestamo_SinPermisosSinPrestamos_DevuelveFalse()
    {
        var tipo = TipoUsuario.SinPermisos;

        var resultado = tipo.PuedeAgregarPrestamo(0);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    public void PermiteUnidades_EstudianteHastaElMaximo_DevuelveTrue(int cantidadSolicitada)
    {
        var tipo = TipoUsuario.Estudiante;
        
        var resultado = tipo.PermiteUnidades(cantidadSolicitada);
        
        Assert.IsTrue(resultado);
    }
    
    [TestMethod]
    [DataRow(3)]
    [DataRow(4)]
    public void PermiteUnidades_EstudianteSobreElMaximo_DevuelveFalse(int cantidadSolicitada)
    {
        var tipo = TipoUsuario.Estudiante;
        
        var resultado = tipo.PermiteUnidades(cantidadSolicitada);
        
        Assert.IsFalse(resultado);
    }
    
    [TestMethod]
    [DataRow(1)]
    [DataRow(40)]
    public void PermiteUnidades_DocenteHastaElMaximo_DevuelveTrue(int cantidadSolicitada)
    {
        var tipo = TipoUsuario.Docente;
    
        var resultado = tipo.PermiteUnidades(cantidadSolicitada);
    
        Assert.IsTrue(resultado);
    }

    [TestMethod]
    [DataRow(41)]
    [DataRow(42)]
    public void PermiteUnidades_DocenteSobreElMaximo_DevuelveFalse(int cantidadSolicitada)
    {
        var tipo = TipoUsuario.Docente;
    
        var resultado = tipo.PermiteUnidades(cantidadSolicitada);
    
        Assert.IsFalse(resultado);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(1000)]
    public void PermiteUnidades_AdministradorConCualquierCantidad_DevuelveTrue(int cantidadSolicitada)
    {
        var tipo = TipoUsuario.Administrador;
    
        var resultado = tipo.PermiteUnidades(cantidadSolicitada);
    
        Assert.IsTrue(resultado);
    }

    [TestMethod]
    public void PermiteUnidades_FuncionarioCEConUnaUnidad_DevuelveFalse()
    {
        var tipo = TipoUsuario.FuncionarioCE;
    
        var resultado = tipo.PermiteUnidades(1);
    
        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void PermiteUnidades_SupervisorConUnaUnidad_DevuelveFalse()
    {
        var tipo = TipoUsuario.Supervisor;
    
        var resultado = tipo.PermiteUnidades(1);
    
        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void PermiteUnidades_SinPermisosConUnaUnidad_DevuelveFalse()
    {
        var tipo = TipoUsuario.SinPermisos;
    
        var resultado = tipo.PermiteUnidades(1);
    
        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void Constructor_DatosValidos_AsignaLosDatos()
    {
        TipoUsuario tipo = CrearTipo();

        Assert.AreEqual("Estudiante", tipo.Nombre);
        Assert.AreEqual("Alumnos de grado", tipo.Descripcion);
        Assert.AreEqual(ImagenPng, tipo.Imagen);
        Assert.AreEqual(2, tipo.MaxUnidadesPorCatalogo);
        Assert.AreEqual(3, tipo.MaxPrestamosConcurrentes);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_DescripcionNulaVaciaOEspacios_LanzaExcepcion(string? descripcion)
    {
        Assert.ThrowsException<DominioException>(() => CrearTipo(descripcion: descripcion!));
    }

    [TestMethod]
    public void Constructor_DescripcionCon101Caracteres_LanzaExcepcion()
    {
        string descripcion = new string('a', 101);

        Assert.ThrowsException<DominioException>(() => CrearTipo(descripcion: descripcion));
    }

    [TestMethod]
    public void Constructor_DescripcionCon100Caracteres_SeCrea()
    {
        string descripcion = new string('a', 100);

        TipoUsuario tipo = CrearTipo(descripcion: descripcion);

        Assert.AreEqual(descripcion, tipo.Descripcion);
    }

    private static TipoUsuario CrearTipo(
        string descripcion = "Alumnos de grado",
        string imagen = ImagenPng,
        int? maxUnidadesPorCatalogo = 2,
        int? maxPrestamosConcurrentes = 3)
    {
        return new TipoUsuario(
            nombre: "Estudiante",
            descripcion: descripcion,
            imagen: imagen,
            maxUnidadesPorCatalogo: maxUnidadesPorCatalogo,
            maxPrestamosConcurrentes: maxPrestamosConcurrentes,
            permisos: new[] { Permiso.SolicitarPrestamo });
    }
}
