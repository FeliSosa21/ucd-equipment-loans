using Data;
using Data.Repositorios;
using Dominio;

namespace Servicios.Tests;

[TestClass]
public class DatosDeEjemploTests
{
    private Memoria _memoria = null!;
    private RepositorioUsuario _usuarios = null!;
    private RepositorioPrestamo _prestamos = null!;
    private DatosDeEjemplo _datos = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _memoria = new Memoria();
        _usuarios = new RepositorioUsuario(_memoria);
        _prestamos = new RepositorioPrestamo(_memoria);
        _datos = new DatosDeEjemplo(_usuarios, _prestamos);
    }

    [TestMethod]
    public void Cargar_MemoriaVacia_GuardaUsuariosConSuTipo()
    {
        _datos.Cargar();

        Assert.AreEqual(TipoUsuario.Docente, _usuarios.ObtenerUsuarioPorCorreo(DatosDeEjemplo.CorreoDocente)!.Tipo);
        Assert.AreEqual(TipoUsuario.Estudiante,
            _usuarios.ObtenerUsuarioPorCorreo(DatosDeEjemplo.CorreoEstudiante)!.Tipo);
        Assert.AreEqual(TipoUsuario.FuncionarioCE,
            _usuarios.ObtenerUsuarioPorCorreo(DatosDeEjemplo.CorreoFuncionario)!.Tipo);
    }

    [TestMethod]
    public void Cargar_MemoriaVacia_GuardaPrestamosActivosConSusUnidadesPrestadas()
    {
        _datos.Cargar();

        IReadOnlyList<Prestamo> activos = _prestamos.ObtenerPrestamosActivos();
        Assert.AreEqual(2, activos.Count);
        Assert.IsTrue(activos.SelectMany(prestamo => prestamo.UnidadesAsignadas)
            .All(unidad => unidad.Estado == EstadoUnidadFisica.Prestada));
    }

    [TestMethod]
    public void Cargar_DosVeces_NoDuplica()
    {
        _datos.Cargar();
        _datos.Cargar();

        Assert.AreEqual(3, _memoria.Usuarios.Count);
        Assert.AreEqual(2, _prestamos.ObtenerPrestamosActivos().Count);
    }
}
