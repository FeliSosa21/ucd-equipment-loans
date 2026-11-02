using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ServicioHistoricoTests
{
    private static readonly DateTime FechaFija = new DateTime(2026, 9, 22, 10, 30, 0);

    [TestMethod]
    public void ListarMovimientos_SinMovimientos_DevuelveListaVacia()
    {
        ServicioHistorico servicio = CrearServicio(new Memoria());

        List<MovimientoDto> movimientos = servicio.ListarMovimientos();

        Assert.AreEqual(0, movimientos.Count);
    }

    [TestMethod]
    public void ListarMovimientos_MovimientoDePrestamo_DevuelveSusDatos()
    {
        Memoria memoria = new Memoria();
        Usuario evaluador = CrearEvaluador();
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0001");
        Prestamo prestamo = CrearPrestamo(requiereAprobacion: true, unidad);
        prestamo.Aprobar(evaluador, new List<UnidadFisica> { unidad });
        new RepositorioMovimiento(memoria).AgregarMovimiento(Movimiento.DePrestamo(prestamo,
            RegistroDeAccion.DeUsuario(evaluador, FechaFija), unidad));
        ServicioHistorico servicio = CrearServicio(memoria);

        MovimientoDto movimiento = servicio.ListarMovimientos().Single();

        Assert.AreEqual(FechaFija, movimiento.FechaHora);
        Assert.AreEqual("Prestamo", movimiento.Tipo);
        Assert.AreEqual("Ana Perez", movimiento.Solicitante);
        Assert.AreEqual("Felipe Sosa", movimiento.Evaluador);
        Assert.AreEqual("EQP-0001", movimiento.Unidad);
    }

    [TestMethod]
    public void ListarMovimientos_MovimientoSinEvaluadorNiUnidad_DevuelveNulos()
    {
        Memoria memoria = new Memoria();
        RegistrarSolicitud(memoria, fechaHora: FechaFija);
        ServicioHistorico servicio = CrearServicio(memoria);

        MovimientoDto movimiento = servicio.ListarMovimientos().Single();

        Assert.IsNull(movimiento.Evaluador);
        Assert.IsNull(movimiento.Unidad);
    }

    [TestMethod]
    public void ListarMovimientos_VariosMovimientos_DevuelveDelMasRecienteAlMasViejo()
    {
        Memoria memoria = new Memoria();
        RegistrarSolicitud(memoria, fechaHora: FechaFija);
        RegistrarSolicitud(memoria, fechaHora: FechaFija.AddDays(2));
        RegistrarSolicitud(memoria, fechaHora: FechaFija.AddDays(1));
        ServicioHistorico servicio = CrearServicio(memoria);

        List<MovimientoDto> movimientos = servicio.ListarMovimientos();

        CollectionAssert.AreEqual(
            new List<DateTime> { FechaFija.AddDays(2), FechaFija.AddDays(1), FechaFija },
            movimientos.Select(movimiento => movimiento.FechaHora).ToList());
    }

    [TestMethod]
    public void ListarMovimientos_MovimientosConLaMismaFechaHora_DevuelvePrimeroElUltimoRegistrado()
    {
        Memoria memoria = new Memoria();
        RegistrarSolicitud(memoria, fechaHora: FechaFija);
        RegistrarPrestamo(memoria, fechaHora: FechaFija);
        ServicioHistorico servicio = CrearServicio(memoria);

        List<MovimientoDto> movimientos = servicio.ListarMovimientos();

        Assert.AreEqual("Prestamo", movimientos[0].Tipo);
    }

    private static ServicioHistorico CrearServicio(Memoria memoria)
    {
        return new ServicioHistorico(new RepositorioMovimiento(memoria));
    }

    private static void RegistrarSolicitud(Memoria memoria, DateTime fechaHora)
    {
        Prestamo prestamo = CrearPrestamo(requiereAprobacion: true, CrearUnidad(idInstitucional: "EQP-0001"));
        new RepositorioMovimiento(memoria).AgregarMovimiento(Movimiento.DeSolicitud(prestamo,
            RegistroDeAccion.DeUsuario(prestamo.Solicitante, fechaHora)));
    }

    private static void RegistrarPrestamo(Memoria memoria, DateTime fechaHora)
    {
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0001");
        Prestamo prestamo = CrearPrestamo(requiereAprobacion: false, unidad);
        prestamo.Activar(new List<UnidadFisica> { unidad });
        new RepositorioMovimiento(memoria).AgregarMovimiento(Movimiento.DePrestamo(prestamo,
            RegistroDeAccion.DelSistema(fechaHora), unidad));
    }

    private static Prestamo CrearPrestamo(bool requiereAprobacion, UnidadFisica unidad)
    {
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: requiereAprobacion, categoria: Categoria.Audiovisual);
        tipo.AgregarUnidad(unidad);

        return new Prestamo(CrearSolicitante(), tipo, cantidadSolicitada: 1, fechaHoraPrestamo: FechaFija);
    }

    private static Usuario CrearSolicitante()
    {
        Usuario usuario = new Usuario(dni: "12345672", nombre: "Ana", apellido: "Perez",
            correo: "ana@hotmail.com", contrasena: "VivaLaOrt2026");
        usuario.AsignarTipo(TipoUsuario.Docente);

        return usuario;
    }

    private static Usuario CrearEvaluador()
    {
        Usuario usuario = new Usuario(dni: "67890127", nombre: "Felipe", apellido: "Sosa",
            correo: "felipe@hotmail.com", contrasena: "VivaLaOrt2026");
        usuario.AsignarTipo(TipoUsuario.Supervisor);

        return usuario;
    }

    private static UnidadFisica CrearUnidad(string idInstitucional)
    {
        return new UnidadFisica(idInstitucional: idInstitucional, ubiFisica: "Deposito A", numSerie: null);
    }
}
