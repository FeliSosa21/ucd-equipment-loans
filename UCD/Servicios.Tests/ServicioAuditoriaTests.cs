using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioAuditoriaTests
{
    private const string DniAdministrador = "11111111";
    private const string DniDocente = "45678905";

    [TestMethod]
    public void ListarRegistros_FiltroNulo_LanzaArgumentNullException()
    {
        ServicioAuditoria servicio = CrearServicio(new Memoria());

        Assert.ThrowsException<ArgumentNullException>(() => servicio.ListarRegistros(null!, DniAdministrador));
    }

    [TestMethod]
    public void ListarRegistros_ResponsableSinPermiso_LanzaServicioException()
    {
        Memoria memoria = new Memoria();
        AgregarUsuario(memoria, DniDocente, TipoUsuario.Docente());
        ServicioAuditoria servicio = CrearServicio(memoria);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarRegistros(new FiltroAuditoriaDto(), DniDocente));
    }

    [TestMethod]
    public void ListarRegistros_ResponsableDesactivado_LanzaServicioException()
    {
        Memoria memoria = new Memoria();
        AgregarUsuario(memoria, DniAdministrador, TipoUsuario.Administrador()).Desactivar();
        ServicioAuditoria servicio = CrearServicio(memoria);

        Assert.ThrowsException<ServicioException>(() =>
            servicio.ListarRegistros(new FiltroAuditoriaDto(), DniAdministrador));
    }

    private static Usuario AgregarUsuario(Memoria memoria, string dni, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni: dni, nombre: "Ana", apellido: "Perez", correo: $"{dni}@hotmail.com",
            contrasena: "VivaLaOrt2026");
        usuario.AsignarTipo(tipo);
        new RepositorioUsuario(memoria).AgregarUsuario(usuario);
        return usuario;
    }

    private static ServicioAuditoria CrearServicio(Memoria memoria)
    {
        return new ServicioAuditoria(new RepositorioAuditoria(memoria), new RepositorioUsuario(memoria));
    }
}
