-- Respaldo vacio de la base UCD: solo la estructura (tablas, indices y claves), sin datos.
-- Se genera desde las migraciones de EF Core (Code First), parado en la raiz del repositorio, con:
--   dotnet ef migrations script --idempotent --project UCD/Data --startup-project UCD/UI
-- Despues se le quitan los 8 bloques de datos de dos migraciones viejas:
--   20261104134751_AgregarTiposDeUsuario (2 INSERT de los tipos de usuario) y
--   20261110190501_RetirarSemillaTiposDeUsuario (6 DELETE de esos mismos tipos).
--   En una base nueva se anulan entre si: no dejan ninguna fila.
-- Las unicas filas que inserta son las de __EFMigrationsHistory, una por migracion (13):
--   son la version del esquema, no datos del sistema. La app llama a Database.Migrate()
--   al arrancar y, sin esas filas, EF intentaria volver a aplicar las migraciones.
-- Uso: sobre una base nueva llamada UCD. Despues, el script de datos de esta carpeta,
--   y despues, la app. Ver la seccion Instalacion de la documentacion.
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021021417_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261021021417_Initial', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028133512_AgregarCatalogos'
)
BEGIN
    CREATE TABLE [Catalogos] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(80) NOT NULL,
        CONSTRAINT [PK_Catalogos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028133512_AgregarCatalogos'
)
BEGIN
    CREATE TABLE [TiposEquipamiento] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(80) NOT NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [DuracionMaxPrestamo] int NOT NULL,
        [RequiereAprobacion] bit NOT NULL,
        [Categoria] nvarchar(max) NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        [CatalogoId] int NOT NULL,
        CONSTRAINT [PK_TiposEquipamiento] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TiposEquipamiento_Catalogos_CatalogoId] FOREIGN KEY ([CatalogoId]) REFERENCES [Catalogos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028133512_AgregarCatalogos'
)
BEGIN
    CREATE TABLE [UnidadesFisicas] (
        [Id] int NOT NULL IDENTITY,
        [IdInstitucional] nvarchar(8) NOT NULL,
        [UbiFisica] nvarchar(80) NOT NULL,
        [NumSerie] nvarchar(50) NULL,
        [Observaciones] nvarchar(300) NULL,
        [Estado] nvarchar(max) NOT NULL,
        [TipoEquipamientoId] int NOT NULL,
        CONSTRAINT [PK_UnidadesFisicas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UnidadesFisicas_TiposEquipamiento_TipoEquipamientoId] FOREIGN KEY ([TipoEquipamientoId]) REFERENCES [TiposEquipamiento] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028133512_AgregarCatalogos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TiposEquipamiento_CatalogoId_Nombre] ON [TiposEquipamiento] ([CatalogoId], [Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028133512_AgregarCatalogos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnidadesFisicas_IdInstitucional] ON [UnidadesFisicas] ([IdInstitucional]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028133512_AgregarCatalogos'
)
BEGIN
    CREATE INDEX [IX_UnidadesFisicas_TipoEquipamientoId] ON [UnidadesFisicas] ([TipoEquipamientoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028133512_AgregarCatalogos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028133512_AgregarCatalogos', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102202258_AgregarUsuarios'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Dni] nvarchar(8) NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        [Apellido] nvarchar(50) NOT NULL,
        [Correo] nvarchar(450) NOT NULL,
        [Contrasena] nvarchar(max) NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        [DebeCambiarContrasena] bit NOT NULL,
        [Tipo] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102202258_AgregarUsuarios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Correo] ON [Usuarios] ([Correo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102202258_AgregarUsuarios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Dni] ON [Usuarios] ([Dni]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102202258_AgregarUsuarios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261102202258_AgregarUsuarios', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103015548_AgregarActivoACatalogos'
)
BEGIN
    ALTER TABLE [Catalogos] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103015548_AgregarActivoACatalogos'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Catalogos_Nombre] ON [Catalogos] ([Nombre]) WHERE [Activo] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103015548_AgregarActivoACatalogos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261103015548_AgregarActivoACatalogos', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103042735_AgregarPrestamos'
)
BEGIN
    CREATE TABLE [Prestamos] (
        [Id] uniqueidentifier NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        [SolicitanteId] int NOT NULL,
        [TipoEquipamientoId] int NOT NULL,
        [CantidadSolicitada] int NOT NULL,
        [FechaHoraPrestamo] datetime2 NOT NULL,
        [FechaEsperadaDevolucion] datetime2 NOT NULL,
        [EvaluadorId] int NULL,
        [MotivoRechazo] nvarchar(300) NULL,
        CONSTRAINT [PK_Prestamos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Prestamos_TiposEquipamiento_TipoEquipamientoId] FOREIGN KEY ([TipoEquipamientoId]) REFERENCES [TiposEquipamiento] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Prestamos_Usuarios_EvaluadorId] FOREIGN KEY ([EvaluadorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Prestamos_Usuarios_SolicitanteId] FOREIGN KEY ([SolicitanteId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103042735_AgregarPrestamos'
)
BEGIN
    CREATE TABLE [UnidadesAsignadas] (
        [PrestamoId] uniqueidentifier NOT NULL,
        [UnidadFisicaId] int NOT NULL,
        CONSTRAINT [PK_UnidadesAsignadas] PRIMARY KEY ([PrestamoId], [UnidadFisicaId]),
        CONSTRAINT [FK_UnidadesAsignadas_Prestamos_PrestamoId] FOREIGN KEY ([PrestamoId]) REFERENCES [Prestamos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UnidadesAsignadas_UnidadesFisicas_UnidadFisicaId] FOREIGN KEY ([UnidadFisicaId]) REFERENCES [UnidadesFisicas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103042735_AgregarPrestamos'
)
BEGIN
    CREATE INDEX [IX_Prestamos_EvaluadorId] ON [Prestamos] ([EvaluadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103042735_AgregarPrestamos'
)
BEGIN
    CREATE INDEX [IX_Prestamos_SolicitanteId] ON [Prestamos] ([SolicitanteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103042735_AgregarPrestamos'
)
BEGIN
    CREATE INDEX [IX_Prestamos_TipoEquipamientoId] ON [Prestamos] ([TipoEquipamientoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103042735_AgregarPrestamos'
)
BEGIN
    CREATE INDEX [IX_UnidadesAsignadas_UnidadFisicaId] ON [UnidadesAsignadas] ([UnidadFisicaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103042735_AgregarPrestamos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261103042735_AgregarPrestamos', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103200344_AgregarMovimientos'
)
BEGIN
    CREATE TABLE [Movimientos] (
        [Id] int NOT NULL IDENTITY,
        [PrestamoId] uniqueidentifier NOT NULL,
        [Tipo] nvarchar(max) NOT NULL,
        [EvaluadorId] int NULL,
        [UnidadId] int NULL,
        [ResponsableId] int NULL,
        [FechaHora] datetime2 NOT NULL,
        CONSTRAINT [PK_Movimientos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Movimientos_Prestamos_PrestamoId] FOREIGN KEY ([PrestamoId]) REFERENCES [Prestamos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Movimientos_UnidadesFisicas_UnidadId] FOREIGN KEY ([UnidadId]) REFERENCES [UnidadesFisicas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Movimientos_Usuarios_EvaluadorId] FOREIGN KEY ([EvaluadorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Movimientos_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103200344_AgregarMovimientos'
)
BEGIN
    CREATE INDEX [IX_Movimientos_EvaluadorId] ON [Movimientos] ([EvaluadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103200344_AgregarMovimientos'
)
BEGIN
    CREATE INDEX [IX_Movimientos_PrestamoId] ON [Movimientos] ([PrestamoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103200344_AgregarMovimientos'
)
BEGIN
    CREATE INDEX [IX_Movimientos_ResponsableId] ON [Movimientos] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103200344_AgregarMovimientos'
)
BEGIN
    CREATE INDEX [IX_Movimientos_UnidadId] ON [Movimientos] ([UnidadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103200344_AgregarMovimientos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261103200344_AgregarMovimientos', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104134751_AgregarTiposDeUsuario'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'Tipo');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Usuarios] DROP COLUMN [Tipo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104134751_AgregarTiposDeUsuario'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [TipoUsuarioId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104134751_AgregarTiposDeUsuario'
)
BEGIN
    CREATE TABLE [TiposDeUsuario] (
        [Id] int NOT NULL IDENTITY,
        [Permisos] nvarchar(max) NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(100) NOT NULL,
        [Imagen] nvarchar(max) NOT NULL,
        [MaxUnidadesPorCatalogo] int NULL,
        [MaxPrestamosConcurrentes] int NULL,
        CONSTRAINT [PK_TiposDeUsuario] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104134751_AgregarTiposDeUsuario'
)
BEGIN
    CREATE INDEX [IX_Usuarios_TipoUsuarioId] ON [Usuarios] ([TipoUsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104134751_AgregarTiposDeUsuario'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_TiposDeUsuario_Nombre] ON [TiposDeUsuario] ([Nombre]) WHERE [Nombre] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104134751_AgregarTiposDeUsuario'
)
BEGIN
    ALTER TABLE [Usuarios] ADD CONSTRAINT [FK_Usuarios_TiposDeUsuario_TipoUsuarioId] FOREIGN KEY ([TipoUsuarioId]) REFERENCES [TiposDeUsuario] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104134751_AgregarTiposDeUsuario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104134751_AgregarTiposDeUsuario', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104142849_AgregarDevoluciones'
)
BEGIN
    CREATE TABLE [Devoluciones] (
        [Id] int NOT NULL IDENTITY,
        [PrestamoId] uniqueidentifier NOT NULL,
        [RegistradaPorId] int NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        CONSTRAINT [PK_Devoluciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Devoluciones_Prestamos_PrestamoId] FOREIGN KEY ([PrestamoId]) REFERENCES [Prestamos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Devoluciones_Usuarios_RegistradaPorId] FOREIGN KEY ([RegistradaPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104142849_AgregarDevoluciones'
)
BEGIN
    CREATE TABLE [UnidadesDevueltas] (
        [Id] int NOT NULL IDENTITY,
        [UnidadFisicaId] int NOT NULL,
        [Condicion] nvarchar(max) NOT NULL,
        [Observaciones] nvarchar(300) NULL,
        [DevolucionId] int NOT NULL,
        CONSTRAINT [PK_UnidadesDevueltas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UnidadesDevueltas_Devoluciones_DevolucionId] FOREIGN KEY ([DevolucionId]) REFERENCES [Devoluciones] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UnidadesDevueltas_UnidadesFisicas_UnidadFisicaId] FOREIGN KEY ([UnidadFisicaId]) REFERENCES [UnidadesFisicas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104142849_AgregarDevoluciones'
)
BEGIN
    CREATE INDEX [IX_Devoluciones_PrestamoId] ON [Devoluciones] ([PrestamoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104142849_AgregarDevoluciones'
)
BEGIN
    CREATE INDEX [IX_Devoluciones_RegistradaPorId] ON [Devoluciones] ([RegistradaPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104142849_AgregarDevoluciones'
)
BEGIN
    CREATE INDEX [IX_UnidadesDevueltas_DevolucionId] ON [UnidadesDevueltas] ([DevolucionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104142849_AgregarDevoluciones'
)
BEGIN
    CREATE INDEX [IX_UnidadesDevueltas_UnidadFisicaId] ON [UnidadesDevueltas] ([UnidadFisicaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104142849_AgregarDevoluciones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104142849_AgregarDevoluciones', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107115743_AgregarEstaEliminadoAUsuarios'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [EstaEliminado] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107115743_AgregarEstaEliminadoAUsuarios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107115743_AgregarEstaEliminadoAUsuarios', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108124328_AgregarRegistrosAuditoria'
)
BEGIN
    CREATE TABLE [RegistrosAuditoria] (
        [Id] int NOT NULL IDENTITY,
        [Accion] nvarchar(max) NOT NULL,
        [Detalle] nvarchar(200) NOT NULL,
        [ResponsableId] int NULL,
        [FechaHora] datetime2 NOT NULL,
        CONSTRAINT [PK_RegistrosAuditoria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RegistrosAuditoria_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108124328_AgregarRegistrosAuditoria'
)
BEGIN
    CREATE INDEX [IX_RegistrosAuditoria_ResponsableId] ON [RegistrosAuditoria] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108124328_AgregarRegistrosAuditoria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261108124328_AgregarRegistrosAuditoria', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110015836_AgregarIndiceNumeroSerie'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UnidadesFisicas_NumSerie] ON [UnidadesFisicas] ([NumSerie]) WHERE [NumSerie] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110015836_AgregarIndiceNumeroSerie'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110015836_AgregarIndiceNumeroSerie', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110190501_RetirarSemillaTiposDeUsuario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110190501_RetirarSemillaTiposDeUsuario', N'9.0.21');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111011344_AgregarNotificaciones'
)
BEGIN
    CREATE TABLE [Notificaciones] (
        [Id] int NOT NULL IDENTITY,
        [PrestamoId] uniqueidentifier NOT NULL,
        [Tipo] nvarchar(max) NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [EstaLeida] bit NOT NULL,
        CONSTRAINT [PK_Notificaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notificaciones_Prestamos_PrestamoId] FOREIGN KEY ([PrestamoId]) REFERENCES [Prestamos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111011344_AgregarNotificaciones'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_PrestamoId] ON [Notificaciones] ([PrestamoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111011344_AgregarNotificaciones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261111011344_AgregarNotificaciones', N'9.0.21');
END;

COMMIT;
GO

