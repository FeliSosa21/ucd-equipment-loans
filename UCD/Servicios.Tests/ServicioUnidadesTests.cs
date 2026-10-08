using Data;
using Data.Repositorios;
using Dominio;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public class ServicioUnidadesTests
{
    private const string DniFuncionario = "12345672";
    private const string DniDocente = "45678905";

    [TestMethod]
    public void Listar_SinFiltros_DevuelveLasUnidadesDeTodosLosTipos()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(new UnidadFisica("PRY-0001", "Deposito A", "SN123"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        UnidadFisica conObservaciones = new UnidadFisica("NTB-0001", "Laboratorio 2", null);
        conObservaciones.ModificarInfo("Laboratorio 2", null, "Pantalla rayada");
        notebook.AgregarUnidad(conObservaciones);
        ServicioUnidades servicio = CrearServicio(proyector, notebook);

        List<UnidadDetalleDto> unidades = servicio.Listar(null, null);

        Assert.AreEqual(2, unidades.Count);
        Assert.AreEqual("PRY-0001", unidades[0].IdInstitucional);
        Assert.AreEqual("Proyector Epson", unidades[0].Tipo);
        Assert.AreEqual("Deposito A", unidades[0].UbicacionFisica);
        Assert.AreEqual("SN123", unidades[0].NumeroSerie);
        Assert.AreEqual("Disponible", unidades[0].Estado);
        Assert.AreEqual("Pantalla rayada", unidades[1].Observaciones);
    }

    [TestMethod]
    public void Listar_FiltroPorTiempo_DevuelveSoloLasDeEseTipo()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        proyector.AgregarUnidad(CrearUnidad("PRY-0001"));
        TipoEquipamiento notebook = CrearTipo("Notebook Lenovo");
        notebook.AgregarUnidad(CrearUnidad("NTB-0001"));
        ServicioUnidades servicio = CrearServicio(proyector, notebook);

        List<UnidadDetalleDto> unidades = servicio.Listar("Notebook Lenovo", null);

        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("NTB-0001", unidades[0].IdInstitucional);
    }

    [TestMethod]
    public void Listar_FiltroPorEstado_DevuelveSoloLasDeEseEstado()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        UnidadFisica enMantenimiento = CrearUnidad("PRY-0002");
        enMantenimiento.AMantenimiento();
        proyector.AgregarUnidad(CrearUnidad("PRY-0001"));
        proyector.AgregarUnidad(enMantenimiento);
        ServicioUnidades servicio = CrearServicio(proyector);

        List<UnidadDetalleDto> unidades = servicio.Listar(null, "EnMantenimiento");

        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("PRY-0002", unidades[0].IdInstitucional);
    }

    [TestMethod]
    public void Registrar_DatosValidos_LaAgregaDisponible()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.Registrar(CrearDatos("Proyector Epson", "PRY-0001", "SN123"), DniFuncionario);

        List<UnidadDetalleDto> unidades = servicio.Listar(null, null);
        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("PRY-0001", unidades[0].IdInstitucional);
        Assert.AreEqual("Proyector Epson", unidades[0].Tipo);
        Assert.AreEqual("Deposito A", unidades[0].UbicacionFisica);
        Assert.AreEqual("SN123", unidades[0].NumeroSerie);
        Assert.AreEqual("Disponible", unidades[0].Estado);
    }

    private static ServicioUnidades CrearServicio(params TipoEquipamiento[] tipos)
    {
        Memoria memoria = new Memoria();
        RepositorioCatalogo repositorioCatalogo = new RepositorioCatalogo(memoria);
        Catalogo catalogo = new Catalogo("Catalogo UCD");
        foreach (TipoEquipamiento tipo in tipos)
        {
            catalogo.AgregarTipo(tipo);
        }

        repositorioCatalogo.Guardar(catalogo);

        RepositorioUsuario repositorioUsuario = new RepositorioUsuario(memoria);
        repositorioUsuario.AgregarUsuario(CrearUsuario(DniFuncionario, "funcionario@ucd.edu.uy",
            TipoUsuario.FuncionarioCE));
        repositorioUsuario.AgregarUsuario(CrearUsuario(DniDocente, "docente@ucd.edu.uy", TipoUsuario.Docente));
        return new ServicioUnidades(repositorioCatalogo, repositorioUsuario);
    }

    private static Usuario CrearUsuario(string dni, string correo, TipoUsuario tipo)
    {
        Usuario usuario = new Usuario(dni, "Nombre", "Apellido", correo, "VivaLaOrt2026");
        usuario.AsignarTipo(tipo);
        return usuario;
    }

    private static TipoEquipamiento CrearTipo(string nombre)
    {
        return new TipoEquipamiento(nombre, "Descripcion", 3, false, Categoria.Audiovisual);
    }

    private static UnidadFisica CrearUnidad(string idInstitucional)
    {
        return new UnidadFisica(idInstitucional, "Deposito A", null);
    }

    [TestMethod]
    public void Registrar_DatosValidos_LaAgregaDisponible()
    {
        TipoEquipamiento proyector = CrearTipo("Proyector Epson");
        ServicioUnidades servicio = CrearServicio(proyector);

        servicio.Registrar(CrearDatos("Proyector Epson", "PRY-0001", "SN123"), DniFuncionario);

        List<UnidadDetalleDto> unidades = servicio.Listar(null, null);
        Assert.AreEqual(1, unidades.Count);
        Assert.AreEqual("PRY-0001", unidades[0].IdInstitucional);
        Assert.AreEqual("Proyector Epson", unidades[0].Tipo);
        Assert.AreEqual("Deposito A", unidades[0].UbicacionFisica);
        Assert.AreEqual("SN123", unidades[0].NumeroSerie);
        Assert.AreEqual("Disponible", unidades[0].Estado);
    }
}