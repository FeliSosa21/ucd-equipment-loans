using Data;
using Data.Repositorios;
using Dominio;

namespace Data.Tests.Repositorios;

[TestClass]
public class RepositorioDevolucionTests
{
    private static readonly DateTime InicioPrestamo = new DateTime(2026, 9, 22, 10, 0, 0);

    [TestMethod]
    public void ObtenerTodas_MemoriaNueva_DevuelveVacia()
    {
        RepositorioDevolucion repositorio = new RepositorioDevolucion(new Memoria());

        IReadOnlyList<Devolucion> devoluciones = repositorio.ObtenerTodas();

        Assert.AreEqual(0, devoluciones.Count);
    }

    [TestMethod]
    public void AgregarDevolucion_Valida_LaGuarda()
    {
        RepositorioDevolucion repositorio = new RepositorioDevolucion(new Memoria());
        Devolucion devolucion = CrearDevolucion();

        repositorio.AgregarDevolucion(devolucion);

        CollectionAssert.AreEqual(new List<Devolucion> { devolucion }, repositorio.ObtenerTodas().ToList());
    }

    [TestMethod]
    public void ObtenerTodas_OtroRepositorioSobreLaMismaMemoria_LaEncuentra()
    {
        Memoria memoria = new Memoria();
        Devolucion devolucion = CrearDevolucion();
        new RepositorioDevolucion(memoria).AgregarDevolucion(devolucion);

        IReadOnlyList<Devolucion> devoluciones = new RepositorioDevolucion(memoria).ObtenerTodas();

        Assert.AreSame(devolucion, devoluciones[0]);
    }

    private static Devolucion CrearDevolucion()
    {
        Usuario solicitante = new Usuario(dni: "45678905", nombre: "Ana", apellido: "Perez",
            correo: "ana@hotmail.com", contrasena: "VivaLaOrt2026");
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: false, categoria: Categoria.Audiovisual);
        Prestamo prestamo = new Prestamo(solicitante, tipo, cantidadSolicitada: 1, fechaHoraPrestamo: InicioPrestamo);
        UnidadFisica unidad = new UnidadFisica(idInstitucional: "EQP-0001", ubiFisica: "Deposito A", numSerie: null);
        tipo.AgregarUnidad(unidad);
        prestamo.Activar(new List<UnidadFisica> { unidad });
        Usuario funcionario = new Usuario(dni: "12345672", nombre: "Felipe", apellido: "Sosa",
            correo: "felipe@hotmail.com", contrasena: "VivaLaOrt2026");

        return new Devolucion(prestamo, RegistroDeAccion.DeUsuario(funcionario, InicioPrestamo.AddDays(1)),
            new List<UnidadDevuelta> { new UnidadDevuelta(unidad, CondicionDevolucion.SinObservaciones, null) });
    }
}
