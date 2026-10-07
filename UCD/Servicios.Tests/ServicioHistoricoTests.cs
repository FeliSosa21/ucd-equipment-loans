using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioHistoricoTests
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
        new RepositorioMovimiento(memoria).AgregarMovimiento(new Movimiento(
            fechaHora: FechaFija,
            tipo: TipoMovimiento.Prestamo,
            solicitante: CrearSolicitante(),
            evaluador: CrearEvaluador(),
            unidad: CrearUnidad(idInstitucional: "EQP-0001")));
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
        new RepositorioMovimiento(memoria).AgregarMovimiento(new Movimiento(
            fechaHora: FechaFija,
            tipo: TipoMovimiento.Solicitud,
            solicitante: CrearSolicitante(),
            evaluador: null,
            unidad: null));
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

    private static ServicioHistorico CrearServicio(Memoria memoria)
    {
        return new ServicioHistorico(new RepositorioMovimiento(memoria));
    }

    private static void RegistrarSolicitud(Memoria memoria, DateTime fechaHora)
    {
        new RepositorioMovimiento(memoria).AgregarMovimiento(new Movimiento(
            fechaHora: fechaHora,
            tipo: TipoMovimiento.Solicitud,
            solicitante: CrearSolicitante(),
            evaluador: null,
            unidad: null));
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
