using Data;
using Data.Repositorios;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioAuditoriaTests
{
    private const string DniAdministrador = "11111111";

    [TestMethod]
    public void ListarRegistros_FiltroNulo_LanzaArgumentNullException()
    {
        ServicioAuditoria servicio = CrearServicio(new Memoria());

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ListarRegistros(null!, DniAdministrador));
    }

    private static ServicioAuditoria CrearServicio(Memoria memoria)
    {
        return new ServicioAuditoria(new RepositorioAuditoria(memoria), new RepositorioUsuario(memoria));
    }
}
