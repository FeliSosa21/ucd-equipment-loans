using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioGestionCatalogosTests
{
    private const string DniSupervisor = "12345672";
    private const string NombreValido = "Laboratorio de Informatica";

    private ContextoDePrueba _entorno = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _entorno = new ContextoDePrueba();
        _entorno.GuardarUsuario(DniSupervisor, TipoUsuario.Supervisor());
    }

    [TestCleanup]
    public void Limpiar()
    {
        _entorno.Dispose();
    }

    [TestMethod]
    public void Crear_NombreValido_SeLeeConOtroContexto()
    {
        ServicioGestionCatalogos servicio = CrearServicio();

        servicio.Crear(NombreValido, DniSupervisor);

        IReadOnlyList<Catalogo> guardados = LeerCatalogosActivos();
        Assert.AreEqual(1, guardados.Count);
        Assert.AreEqual(NombreValido, guardados[0].Nombre);
    }

    private ServicioGestionCatalogos CrearServicio()
    {
        return new ServicioGestionCatalogos(_entorno.Catalogos, _entorno.Usuarios, _entorno.Prestamos);
    }

    private IReadOnlyList<Catalogo> LeerCatalogosActivos()
    {
        using SqlContext lectura = _entorno.NuevoContexto();
        return new RepositorioCatalogoSql(lectura).ObtenerCatalogosActivos();
    }
}
