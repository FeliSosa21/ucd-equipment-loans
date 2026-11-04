using System.Text;
using DTOs;

namespace Servicios.Tests;

[TestClass]
public sealed class ExportadorCsvTests
{
    private const char MarcaDeOrdenDeBytes = '\uFEFF';

    [TestMethod]
    public void Exportar_TablaSinFilas_EscribeSoloElEncabezado()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(encabezados: new List<string> { "Nombre", "Edad" });

        byte[] bytes = exportador.Exportar(tabla);

        Assert.AreEqual("Nombre,Edad\r\n", LeerTexto(bytes));
    }

    [TestMethod]
    public void Formato_ExportadorCsv_DevuelveCSV()
    {
        ExportadorCsv exportador = new ExportadorCsv();

        string formato = exportador.Formato;

        Assert.AreEqual("CSV", formato);
    }

    [TestMethod]
    public void Extension_ExportadorCsv_DevuelvePuntoCsv()
    {
        ExportadorCsv exportador = new ExportadorCsv();

        string extension = exportador.Extension;

        Assert.AreEqual(".csv", extension);
    }

    [TestMethod]
    public void TipoContenido_ExportadorCsv_DevuelveTextCsv()
    {
        ExportadorCsv exportador = new ExportadorCsv();

        string tipoContenido = exportador.TipoContenido;

        Assert.AreEqual("text/csv", tipoContenido);
    }

    [TestMethod]
    public void Exportar_TablaConUnaFila_EscribeEncabezadoYFila()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(
            encabezados: new List<string> { "Nombre", "Edad" },
            filas: new List<List<string>> { new List<string> { "Ana", "20" } });

        byte[] bytes = exportador.Exportar(tabla);

        Assert.AreEqual("Nombre,Edad\r\nAna,20\r\n", LeerTexto(bytes));
    }

    [TestMethod]
    public void Exportar_CampoConComa_LoEscribeEntreComillas()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(
            encabezados: new List<string> { "Nombre", "Observaciones" },
            filas: new List<List<string>> { new List<string> { "Ana", "Rayado, sin tapa" } });

        byte[] bytes = exportador.Exportar(tabla);

        Assert.AreEqual("Nombre,Observaciones\r\nAna,\"Rayado, sin tapa\"\r\n", LeerTexto(bytes));
    }

    [TestMethod]
    public void Exportar_CampoConComillas_DuplicaLasComillasInternas()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(
            encabezados: new List<string> { "Nombre", "Observaciones" },
            filas: new List<List<string>> { new List<string> { "Ana", "Cable \"largo\" roto" } });

        byte[] bytes = exportador.Exportar(tabla);

        Assert.AreEqual("Nombre,Observaciones\r\nAna,\"Cable \"\"largo\"\" roto\"\r\n", LeerTexto(bytes));
    }

    [TestMethod]
    public void Exportar_CampoConSaltoDeLinea_LoEscribeEntreComillas()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(
            encabezados: new List<string> { "Nombre", "Observaciones" },
            filas: new List<List<string>> { new List<string> { "Ana", "Primera linea\r\nSegunda linea" } });

        byte[] bytes = exportador.Exportar(tabla);

        Assert.AreEqual("Nombre,Observaciones\r\nAna,\"Primera linea\r\nSegunda linea\"\r\n", LeerTexto(bytes));
    }

    [TestMethod]
    public void Exportar_CeldasVacias_LasDejaVaciasSinComillas()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(
            encabezados: new List<string> { "Nombre", "Edad", "Observaciones" },
            filas: new List<List<string>> { new List<string> { "Ana", "", "" }, new List<string> { "", "30", "Sin rayones" } });

        byte[] bytes = exportador.Exportar(tabla);

        Assert.AreEqual("Nombre,Edad,Observaciones\r\nAna,,\r\n,30,Sin rayones\r\n", LeerTexto(bytes));
    }

    [TestMethod]
    public void Exportar_CualquierTabla_EmpiezaConLaMarcaDeOrdenDeBytesUtf8()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(encabezados: new List<string> { "Nombre", "Edad" });
        byte[] marcaEsperada = new byte[] { 0xEF, 0xBB, 0xBF };

        byte[] bytes = exportador.Exportar(tabla);

        CollectionAssert.AreEqual(marcaEsperada, bytes[..3]);
    }

    [TestMethod]
    public void Exportar_TablaNula_LanzaArgumentNullException()
    {
        ExportadorCsv exportador = new ExportadorCsv();

        Assert.ThrowsException<ArgumentNullException>(() => exportador.Exportar(null!));
    }

    [TestMethod]
    public void Exportar_FilaConMasColumnasQueEncabezados_LanzaArgumentException()
    {
        ExportadorCsv exportador = new ExportadorCsv();
        TablaExportableDto tabla = CrearTabla(
            encabezados: new List<string> { "Nombre", "Edad" },
            filas: new List<List<string>> { new List<string> { "Ana", "20" }, new List<string> { "Luis", "30", "Sobrante" } });

        Assert.ThrowsException<ArgumentException>(() => exportador.Exportar(tabla));
    }

    private static TablaExportableDto CrearTabla(List<string> encabezados, List<List<string>>? filas = null)
    {
        TablaExportableDto tabla = new TablaExportableDto();
        tabla.Encabezados = encabezados;
        if (filas != null)
        {
            tabla.Filas = filas;
        }
        return tabla;
    }

    private static string LeerTexto(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes).TrimStart(MarcaDeOrdenDeBytes);
    }
}
