namespace Dominio.Tests;

[TestClass]
public sealed class DevolucionTests
{
    private static readonly DateTime InicioPrestamo = new DateTime(2026, 9, 22, 10, 0, 0);

    [TestMethod]
    public void Constructor_DatosValidos_GuardaLosDatos()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        Usuario funcionario = CrearFuncionario();
        DateTime fecha = InicioPrestamo.AddDays(3);
        List<UnidadDevuelta> unidades = DevolverTodasSinObservaciones(prestamo);

        Devolucion devolucion = new Devolucion(prestamo, RegistroDeAccion.DeUsuario(funcionario, fecha), unidades);

        Assert.AreSame(prestamo, devolucion.Prestamo);
        Assert.AreSame(funcionario, devolucion.RegistradaPor);
        Assert.AreEqual(fecha, devolucion.FechaHoraDevolucion);
        CollectionAssert.AreEqual(unidades, devolucion.Unidades.ToList());
    }

    [TestMethod]
    public void Constructor_PrestamoNulo_LanzaExcepcion()
    {
        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(null!, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo), new List<UnidadDevuelta>()));
    }

    [TestMethod]
    public void Constructor_PrestamoSolicitado_LanzaExcepcion()
    {
        Prestamo solicitado = CrearPrestamoSolicitado();

        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(solicitado, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo), new List<UnidadDevuelta>()));
    }

    [TestMethod]
    public void Constructor_SinUsuarioQueRegistra_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoActivo();

        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(prestamo, RegistroDeAccion.DelSistema(InicioPrestamo), DevolverTodasSinObservaciones(prestamo)));
    }

    [TestMethod]
    public void Constructor_FechaAnteriorAlInicioDelPrestamo_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoActivo();

        Assert.ThrowsException<DominioException>(() => new Devolucion(prestamo,
            RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo.AddMinutes(-1)), DevolverTodasSinObservaciones(prestamo)));
    }

    [TestMethod]
    public void Constructor_FechaIgualAlInicioDelPrestamo_SeCrea()
    {
        Prestamo prestamo = CrearPrestamoActivo();

        Devolucion devolucion = new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo),
            DevolverTodasSinObservaciones(prestamo));

        Assert.AreEqual(InicioPrestamo, devolucion.FechaHoraDevolucion);
    }

    [TestMethod]
    public void Constructor_UnidadesNulas_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoActivo();

        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo), null!));
    }

    [TestMethod]
    public void Constructor_UnidadDevueltaNula_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        List<UnidadDevuelta> unidades = DevolverTodasSinObservaciones(prestamo);
        unidades[1] = null!;

        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo), unidades));
    }

    [TestMethod]
    public void Constructor_FaltaUnaUnidadDelPrestamo_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        List<UnidadDevuelta> soloUna = DevolverTodasSinObservaciones(prestamo).Take(1).ToList();

        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo), soloUna));
    }

    [TestMethod]
    public void Constructor_UnidadQueNoEsDelPrestamo_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        List<UnidadDevuelta> unidades = DevolverTodasSinObservaciones(prestamo);
        UnidadFisica ajena = CrearUnidad("EQP-0099");
        ajena.Prestada();
        unidades[1] = new UnidadDevuelta(ajena, CondicionDevolucion.SinObservaciones, null);

        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo), unidades));
    }

    [TestMethod]
    public void Constructor_UnidadRepetida_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        UnidadFisica primera = prestamo.UnidadesAsignadas[0];
        List<UnidadDevuelta> repetidas = new List<UnidadDevuelta>
        {
            new UnidadDevuelta(primera, CondicionDevolucion.SinObservaciones, null),
            new UnidadDevuelta(primera, CondicionDevolucion.ConFallas, null)
        };

        Assert.ThrowsException<DominioException>(() =>
            new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo), repetidas));
    }

    [TestMethod]
    public void Aplicar_CondicionPorUnidad_ActualizaCadaUnidadSegunSuCondicion()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        UnidadFisica sana = prestamo.UnidadesAsignadas[0];
        UnidadFisica rota = prestamo.UnidadesAsignadas[1];
        Devolucion devolucion = new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo.AddDays(1)),
            new List<UnidadDevuelta>
            {
                new UnidadDevuelta(sana, CondicionDevolucion.SinObservaciones, null),
                new UnidadDevuelta(rota, CondicionDevolucion.Daniada, "No enciende")
            });

        devolucion.Aplicar();

        Assert.AreEqual(EstadoUnidadFisica.Disponible, sana.Estado);
        Assert.AreEqual(EstadoUnidadFisica.EnMantenimiento, rota.Estado);
    }

    [TestMethod]
    public void Aplicar_DevolucionValida_DejaElPrestamoDevuelto()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        Devolucion devolucion = new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo.AddDays(1)),
            DevolverTodasSinObservaciones(prestamo));

        devolucion.Aplicar();

        Assert.AreEqual(EstadoPrestamo.Devuelto, prestamo.Estado);
    }

    [TestMethod]
    public void Aplicar_DosVeces_LanzaExcepcion()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        Devolucion devolucion = new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo),
            DevolverTodasSinObservaciones(prestamo));
        devolucion.Aplicar();

        Assert.ThrowsException<DominioException>(() => devolucion.Aplicar());
        Assert.AreEqual(EstadoPrestamo.Devuelto, prestamo.Estado);
    }

    [TestMethod]
    public void Aplicar_UnaUnidadYaNoEstaPrestada_LanzaExcepcionYNoCambiaNada()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        UnidadFisica primera = prestamo.UnidadesAsignadas[0];
        UnidadFisica segunda = prestamo.UnidadesAsignadas[1];
        Devolucion devolucion = new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo),
            DevolverTodasSinObservaciones(prestamo));
        segunda.Disponible();

        Assert.ThrowsException<DominioException>(() => devolucion.Aplicar());
        Assert.AreEqual(EstadoUnidadFisica.Prestada, primera.Estado);
        Assert.AreEqual(EstadoPrestamo.Activo, prestamo.Estado);
    }

    [TestMethod]
    public void Constructor_SolicitanteDesactivado_SeCrea()
    {
        Prestamo prestamo = CrearPrestamoActivo();
        prestamo.Solicitante.Desactivar();

        Devolucion devolucion = new Devolucion(prestamo, RegistroDeAccion.DeUsuario(CrearFuncionario(), InicioPrestamo),
            DevolverTodasSinObservaciones(prestamo));

        Assert.AreSame(prestamo, devolucion.Prestamo);
    }

    private static List<UnidadDevuelta> DevolverTodasSinObservaciones(Prestamo prestamo)
    {
        return prestamo.UnidadesAsignadas
            .Select(unidad => new UnidadDevuelta(unidad, CondicionDevolucion.SinObservaciones, null))
            .ToList();
    }

    private static Prestamo CrearPrestamoSolicitado()
    {
        Usuario solicitante = new Usuario(dni: "45678905", nombre: "Ana", apellido: "Perez",
            correo: "ana@hotmail.com", contrasena: "VivaLaOrt2026");
        TipoEquipamiento tipo = new TipoEquipamiento(nombre: "Proyector Epson", descripcion: "Proyector para aulas",
            duracionMaxPrestamo: 5, requiereAprobacion: false, categoria: Categoria.Audiovisual);
        return new Prestamo(solicitante, tipo, cantidadSolicitada: 2, fechaHoraPrestamo: InicioPrestamo);
    }

    private static Prestamo CrearPrestamoActivo()
    {
        Prestamo prestamo = CrearPrestamoSolicitado();
        List<UnidadFisica> unidades = new List<UnidadFisica> { CrearUnidad("EQP-0001"), CrearUnidad("EQP-0002") };
        foreach (UnidadFisica unidad in unidades)
        {
            prestamo.TipoEquipamiento.AgregarUnidad(unidad);
        }
        prestamo.Activar(unidades);
        return prestamo;
    }

    private static UnidadFisica CrearUnidad(string idInstitucional)
    {
        return new UnidadFisica(idInstitucional: idInstitucional, ubiFisica: "Deposito A", numSerie: null);
    }

    private static Usuario CrearFuncionario()
    {
        return new Usuario(dni: "12345672", nombre: "Felipe", apellido: "Sosa", correo: "felipe@hotmail.com",
            contrasena: "VivaLaOrt2026");
    }
}
