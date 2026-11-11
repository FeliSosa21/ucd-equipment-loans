using System.Runtime.CompilerServices;
using Data;
using Data.Repositorios;
using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Servicios.Tests;

internal static class AuditoriaDePrueba
{
    public static readonly DateTime Ahora = new(2026, 11, 8, 9, 30, 0);

    // Solo devoluciones conserva Memoria hasta el pase completo a EF.
    private static readonly ConditionalWeakTable<Memoria, DbContextOptions<SqlContext>> _bases = new();

    public static SqlContext CrearContexto(Memoria memoria)
    {
        return new SqlContext(Opciones(memoria));
    }

    public static RegistradorDeAuditoria Registrador(SqlContext contexto)
    {
        return new RegistradorDeAuditoria(new RepositorioAuditoriaSql(contexto), new RelojFijo(Ahora));
    }

    public static IReadOnlyList<RegistroAuditoria> LeerRegistros(Memoria memoria, FiltroAuditoria filtro)
    {
        using SqlContext contexto = new SqlContext(Opciones(memoria));
        return new RepositorioAuditoriaSql(contexto).ObtenerRegistros(filtro);
    }

    private static DbContextOptions<SqlContext> Opciones(Memoria memoria)
    {
        return _bases.GetValue(memoria, _ => new DbContextOptionsBuilder<SqlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    }
}
