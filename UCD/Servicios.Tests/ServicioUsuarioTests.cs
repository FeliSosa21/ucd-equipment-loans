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
        UsuarioDto dto = CrearDto();

        servicio.Registrar(dto);

        Assert.IsNotNull(repositorio.ObtenerUsuarioPorDni(dto.Dni));
    }

    [TestMethod]
    public void Registrar_DniDuplicado_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio);
        UsuarioDto primero = CrearDto();
        UsuarioDto repetido = CrearDto(correo: "otro@hotmail.com");
        servicio.Registrar(primero);

        Assert.ThrowsException<DominioException>(() => servicio.Registrar(repetido));
    }

    [TestMethod]
    public void Registrar_CorreoDuplicadoConOtrasMayusculas_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio);
        UsuarioDto primero = CrearDto();
        UsuarioDto repetido = CrearDto(dni: "45678905", correo: "Felipe@Hotmail.COM");
        servicio.Registrar(primero);

        Assert.ThrowsException<DominioException>(() => servicio.Registrar(repetido));
    }

    [TestMethod]
    public void Registrar_DatosInvalidos_LanzaExcepcion()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio);
        UsuarioDto dto = CrearDto(dni: "12345678");


        Assert.ThrowsException<DominioException>(() => servicio.Registrar(dto));
    }

    [TestMethod]
    public void Registrar_DatosValidos_GuardaLosDatosDto()
    {
        Memoria memoria = new Memoria();
        RepositorioUsuario repositorio = new RepositorioUsuario(memoria);
        ServicioUsuario servicio = new ServicioUsuario(repositorio);
        UsuarioDto dto = CrearDto();
        
        servicio.Registrar(dto);
        Usuario? guardado = repositorio.ObtenerUsuarioPorDni(dto.Dni);
        
        Assert.AreEqual(dto.Apellido, guardado!.Apellido);
        Assert.AreEqual(dto.Correo, guardado!.Correo);
        Assert.AreEqual(dto.Nombre, guardado!.Nombre);
    }

    private static UsuarioDto CrearDto(string dni = "12345672", string correo = "felipe@hotmail.com")
    {
        return new UsuarioDto
        {
            Dni = dni, Nombre = "Felipe", Apellido = "Sosa", Correo = correo, Contrasena = "VivaLaOrt2026"
        };
    }
}