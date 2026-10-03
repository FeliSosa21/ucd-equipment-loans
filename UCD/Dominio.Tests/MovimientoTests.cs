namespace Dominio.Tests;

[TestClass]
public sealed class MovimientoTests
{
    private static readonly DateTime FechaFija = new(2026, 10, 5, 10, 0, 0);

    [TestMethod]
    public void Constructor_DatosValidos_ConservaFechaSolicitanteEvaluadorYUnidad()
    {
        Usuario solicitante = CrearSolicitante();
        Usuario evaluador = CrearEvaluador();
        UnidadFisica unidad = CrearUnidad(idInstitucional: "EQP-0001");

        Movimiento movimiento = CrearMovimiento(
            solicitante: solicitante,
            evaluador: evaluador,
            unidad: unidad);

        Assert.AreEqual(FechaFija, movimiento.FechaHora);
        Assert.AreSame(solicitante, movimiento.Solicitante);
        Assert.AreSame(evaluador, movimiento.Evaluador);
        Assert.AreSame(unidad, movimiento.Unidad);
    }

    private static Usuario CrearSolicitante()
    {
        Usuario usuario = new Usuario(
            dni: "12345672",
            nombre: "Renato",
            apellido: "Alves",
            correo: "renato.alves@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Docente);

        return usuario;
    }

    private static Usuario CrearEvaluador()
    {
        Usuario usuario = new Usuario(
            dni: "45678905",
            nombre: "Ana",
            apellido: "Perez",
            correo: "ana.perez@ejemplo.com",
            contrasena: "VivalaOrt2026");

        usuario.AsignarTipo(TipoUsuario.Supervisor);

        return usuario;
    }

    private static UnidadFisica CrearUnidad(string idInstitucional)
    {
        UnidadFisica unidad = new UnidadFisica(
            idInstitucional: idInstitucional,
            ubiFisica: "Laboratorio Prueba",
            numSerie: null);

        return unidad;
    }

    private static Movimiento CrearMovimiento(
        Usuario? solicitante = null,
        Usuario? evaluador = null,
        UnidadFisica? unidad = null)
    {
        Movimiento movimiento = new Movimiento(
            fechaHora: FechaFija,
            tipo: TipoMovimiento.Prestamo,
            solicitante: solicitante ?? CrearSolicitante(),
            evaluador: evaluador,
            unidad: unidad ?? CrearUnidad(idInstitucional: "EQP-0001"));

        return movimiento;
    }
}
