using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioUsuarioTests
{
    [TestMethod]
    public void Registrar_DatosValidos_SeGuarda()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio);
        UsuarioDto dto = new UsuarioDto
        {
            Dni = "12345672", Nombre = "Felipe", Apellido = "Sosa", Correo = "felipe@hotmail.com",
            Contrasena = "VivaLaOrt2026"
        };

        servicio.Registrar(dto);

        Assert.IsNotNull(repositorio.ObtenerUsuarioPorDni(dto.Dni));
    }

    [TestMethod]
    public void Registrar_DniDuplicado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio);
        UsuarioDto dto1 = new UsuarioDto
        {
            Dni = "12345672", Nombre = "Felipe", Apellido = "Sosa", Correo = "felipe@hotmail.com",
            Contrasena = "VivaLaOrt2026"
        };
        UsuarioDto repetido = new UsuarioDto
        {
            Dni = "12345672", Nombre = "Felipe", Apellido = "Sosa", Correo = "felipe@hotmail.com",
            Contrasena = "VivaLaOrt2026"
        };
        servicio.Registrar(dto1);
        Assert.ThrowsException<DominioException>(() => servicio.Registrar(repetido));
    }

    [TestMethod]
    public void Registrar_CorreoDuplicadoConOtrasMayusculas_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio);
        UsuarioDto dto1 = new UsuarioDto
        {
            Dni = "12345672", Nombre = "Felipe", Apellido = "Sosa", Correo = "felipe@hotmail.com",
            Contrasena = "VivaLaOrt2026"
        };
        UsuarioDto repetido = new UsuarioDto
        {
            Dni = "54670643", Nombre = "Felipe", Apellido = "Sosa", Correo = "Felipe@hotmail.COM",
            Contrasena = "VivaLaOrt2026"
        };
        servicio.Registrar(dto1);
        Assert.ThrowsException<DominioException>(() => servicio.Registrar(repetido));
        
    }
}