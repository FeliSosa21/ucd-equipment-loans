-- Respaldo con datos de prueba de la base UCD (Obligatorio 2 de DA1).
-- Se corre DESPUES del respaldo vacio (UCD_vacio.sql), sobre una base nueva llamada UCD.
-- Trae los 6 tipos de usuario (Id 1 a 6), el root y los datos de prueba: usuarios, catalogos,
--   tipos de equipamiento, unidades, prestamos, movimientos, devoluciones, notificaciones y log.
-- No trae filas de __EFMigrationsHistory: ya las deja el respaldo vacio.
-- Se genero exportando la base con DBeaver (Data export, formato SQL, un INSERT por fila)
--   y ordenando las tablas segun sus claves foraneas, con IDENTITY_INSERT en las tablas con identidad.
-- Va entero en una transaccion con XACT_ABORT: si una sentencia falla, no queda nada a medias.
-- Ver la seccion Instalacion de la documentacion.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Catalogos (3 filas)
SET IDENTITY_INSERT [Catalogos] ON;
INSERT INTO Catalogos (Id,Nombre,Activo) VALUES (1,N'Electrónica',1);
INSERT INTO Catalogos (Id,Nombre,Activo) VALUES (2,N'Biotecnología',1);
INSERT INTO Catalogos (Id,Nombre,Activo) VALUES (3,N'Química',0);
SET IDENTITY_INSERT [Catalogos] OFF;

-- TiposEquipamiento (14 filas)
SET IDENTITY_INSERT [TiposEquipamiento] ON;
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (1,N'Osciloscopio digital',N'Osciloscopio de 4 canales y 100 MHz para medir y visualizar señales en prácticas de circuitos.',5,1,N'Electronica',N'Activo',1);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (2,N'Multímetro',N'Multímetro digital de mano para tensión, corriente, resistencia y continuidad.',7,0,N'Electronica',N'Activo',1);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (3,N'Fuente de alimentación',N'Fuente de laboratorio regulable de 0 a 30 V y 0 a 5 A con dos salidas.',5,0,N'Electronica',N'Activo',1);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (4,N'Microscopio',N'Microscopio estereoscópico para inspección de placas y soldaduras.',3,0,N'Laboratorio',N'Activo',1);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (5,N'Laptop',N'Laptop con software de simulación de circuitos (LTspice, KiCad) instalado.',7,1,N'Computacion',N'Activo',1);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (6,N'Microscopio',N'Microscopio óptico binocular de 40x a 1000x para observación de muestras biológicas.',5,0,N'Laboratorio',N'Activo',2);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (7,N'Centrífuga',N'Centrífuga de mesa de hasta 15000 rpm para tubos de 1,5 y 2 ml.',3,1,N'Laboratorio',N'Activo',2);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (8,N'Micropipeta',N'Micropipeta de volumen variable de 10 a 100 µl.',10,0,N'Laboratorio',N'Activo',2);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (9,N'Proyector',N'Proyector portátil para presentaciones de laboratorio y defensas de proyectos.',2,0,N'Audiovisual',N'Activo',2);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (10,N'Kit de electroforesis',N'Cuba horizontal con fuente para electroforesis en gel de agarosa (modelo anterior, en reemplazo).',5,1,N'Laboratorio',N'Discontinuado',2);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (11,N'Balanza analítica',N'Balanza analítica de precisión 0,0001 g con cámara de pesada.',3,1,N'Laboratorio',N'Activo',3);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (12,N'pHmetro',N'pHmetro de mesa con electrodo combinado y compensación de temperatura.',7,0,N'Laboratorio',N'Activo',3);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (13,N'Agitador magnético',N'Agitador magnético con calefacción hasta 340 °C.',7,0,N'Laboratorio',N'Activo',3);
INSERT INTO TiposEquipamiento (Id,Nombre,Descripcion,DuracionMaxPrestamo,RequiereAprobacion,Categoria,Estado,CatalogoId) VALUES (14,N'Espectrofotómetro',N'Espectrofotómetro UV-Vis de 190 a 1100 nm para análisis cuantitativo.',3,1,N'Laboratorio',N'Activo',3);
SET IDENTITY_INSERT [TiposEquipamiento] OFF;

-- UnidadesFisicas (84 filas)
SET IDENTITY_INSERT [UnidadesFisicas] ON;
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (1,N'OSC-0001',N'Laboratorio de Electrónica - Armario A',N'DS1Z4A00001',NULL,N'Prestada',1);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (2,N'OSC-0002',N'Laboratorio de Electrónica - Armario A',N'DS1Z4A00002',NULL,N'Disponible',1);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (3,N'OSC-0003',N'Laboratorio de Electrónica - Armario A',N'DS1Z4A00003',NULL,N'Disponible',1);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (4,N'OSC-0004',N'Laboratorio de Electrónica - Armario A',NULL,NULL,N'Disponible',1);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (5,N'OSC-0005',N'Laboratorio de Electrónica - Armario A',N'DS1Z4A00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',1);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (6,N'OSC-0006',N'Laboratorio de Electrónica - Armario A',N'DS1Z4A00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',1);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (7,N'MUL-0001',N'Laboratorio de Electrónica - Cajonera 2',N'UT61E-00001',NULL,N'Disponible',2);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (8,N'MUL-0002',N'Laboratorio de Electrónica - Cajonera 2',N'UT61E-00002',NULL,N'Prestada',2);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (9,N'MUL-0003',N'Laboratorio de Electrónica - Cajonera 2',N'UT61E-00003',NULL,N'Prestada',2);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (10,N'MUL-0004',N'Laboratorio de Electrónica - Cajonera 2',NULL,NULL,N'Disponible',2);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (11,N'MUL-0005',N'Laboratorio de Electrónica - Cajonera 2',N'UT61E-00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',2);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (12,N'MUL-0006',N'Laboratorio de Electrónica - Cajonera 2',N'UT61E-00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',2);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (13,N'FUE-0001',N'Laboratorio de Electrónica - Estante B',N'PS305D00001',NULL,N'Prestada',3);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (14,N'FUE-0002',N'Laboratorio de Electrónica - Estante B',N'PS305D00002',NULL,N'Prestada',3);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (15,N'FUE-0003',N'Laboratorio de Electrónica - Estante B',N'PS305D00003',NULL,N'Disponible',3);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (16,N'FUE-0004',N'Laboratorio de Electrónica - Estante B',NULL,NULL,N'Disponible',3);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (17,N'FUE-0005',N'Laboratorio de Electrónica - Estante B',N'PS305D00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',3);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (18,N'FUE-0006',N'Laboratorio de Electrónica - Estante B',N'PS305D00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',3);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (19,N'MIE-0001',N'Laboratorio de Electrónica - Mesa de inspección',N'SZM45-00001',NULL,N'Disponible',4);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (20,N'MIE-0002',N'Laboratorio de Electrónica - Mesa de inspección',N'SZM45-00002',NULL,N'Disponible',4);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (21,N'MIE-0003',N'Laboratorio de Electrónica - Mesa de inspección',N'SZM45-00003',NULL,N'Disponible',4);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (22,N'MIE-0004',N'Laboratorio de Electrónica - Mesa de inspección',NULL,NULL,N'Disponible',4);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (23,N'MIE-0005',N'Laboratorio de Electrónica - Mesa de inspección',N'SZM45-00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',4);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (24,N'MIE-0006',N'Laboratorio de Electrónica - Mesa de inspección',N'SZM45-00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',4);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (25,N'LAP-0001',N'Laboratorio de Electrónica - Carro de carga',N'5CG200001',NULL,N'Disponible',5);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (26,N'LAP-0002',N'Laboratorio de Electrónica - Carro de carga',N'5CG200002',NULL,N'Disponible',5);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (27,N'LAP-0003',N'Laboratorio de Electrónica - Carro de carga',N'5CG200003',NULL,N'Disponible',5);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (28,N'LAP-0004',N'Laboratorio de Electrónica - Carro de carga',NULL,NULL,N'Disponible',5);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (29,N'LAP-0005',N'Laboratorio de Electrónica - Carro de carga',N'5CG200005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',5);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (30,N'LAP-0006',N'Laboratorio de Electrónica - Carro de carga',N'5CG200006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',5);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (31,N'MIB-0001',N'Laboratorio de Biotecnología - Armario 1',N'CX23B-00001',NULL,N'Disponible',6);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (32,N'MIB-0002',N'Laboratorio de Biotecnología - Armario 1',N'CX23B-00002',NULL,N'Disponible',6);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (33,N'MIB-0003',N'Laboratorio de Biotecnología - Armario 1',N'CX23B-00003',NULL,N'Disponible',6);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (34,N'MIB-0004',N'Laboratorio de Biotecnología - Armario 1',NULL,NULL,N'Disponible',6);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (35,N'MIB-0005',N'Laboratorio de Biotecnología - Armario 1',N'CX23B-00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',6);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (36,N'MIB-0006',N'Laboratorio de Biotecnología - Armario 1',N'CX23B-00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',6);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (37,N'CEN-0001',N'Laboratorio de Biotecnología - Mesada central',N'MC15K-00001',NULL,N'Prestada',7);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (38,N'CEN-0002',N'Laboratorio de Biotecnología - Mesada central',N'MC15K-00002',NULL,N'Disponible',7);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (39,N'CEN-0003',N'Laboratorio de Biotecnología - Mesada central',N'MC15K-00003',NULL,N'Disponible',7);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (40,N'CEN-0004',N'Laboratorio de Biotecnología - Mesada central',NULL,NULL,N'Disponible',7);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (41,N'CEN-0005',N'Laboratorio de Biotecnología - Mesada central',N'MC15K-00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',7);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (42,N'CEN-0006',N'Laboratorio de Biotecnología - Mesada central',N'MC15K-00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',7);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (43,N'PIP-0001',N'Laboratorio de Biotecnología - Cajonera 3',N'PV100-00001',NULL,N'Disponible',8);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (44,N'PIP-0002',N'Laboratorio de Biotecnología - Cajonera 3',N'PV100-00002',NULL,N'Disponible',8);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (45,N'PIP-0003',N'Laboratorio de Biotecnología - Cajonera 3',N'PV100-00003',NULL,N'Disponible',8);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (46,N'PIP-0004',N'Laboratorio de Biotecnología - Cajonera 3',NULL,NULL,N'Disponible',8);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (47,N'PIP-0005',N'Laboratorio de Biotecnología - Cajonera 3',N'PV100-00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',8);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (48,N'PIP-0006',N'Laboratorio de Biotecnología - Cajonera 3',N'PV100-00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',8);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (49,N'PRY-0001',N'Laboratorio de Biotecnología - Depósito',N'EB770F00001',NULL,N'Disponible',9);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (50,N'PRY-0002',N'Laboratorio de Biotecnología - Depósito',N'EB770F00002',NULL,N'Disponible',9);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (51,N'PRY-0003',N'Laboratorio de Biotecnología - Depósito',N'EB770F00003',NULL,N'Disponible',9);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (52,N'PRY-0004',N'Laboratorio de Biotecnología - Depósito',NULL,NULL,N'Disponible',9);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (53,N'PRY-0005',N'Laboratorio de Biotecnología - Depósito',N'EB770F00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',9);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (54,N'PRY-0006',N'Laboratorio de Biotecnología - Depósito',N'EB770F00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',9);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (55,N'ELF-0001',N'Laboratorio de Biotecnología - Estante C',N'GEL20-00001',NULL,N'Disponible',10);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (56,N'ELF-0002',N'Laboratorio de Biotecnología - Estante C',N'GEL20-00002',NULL,N'Disponible',10);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (57,N'ELF-0003',N'Laboratorio de Biotecnología - Estante C',N'GEL20-00003',NULL,N'Disponible',10);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (58,N'ELF-0004',N'Laboratorio de Biotecnología - Estante C',NULL,N'Modelo discontinuado; retirado del uso.',N'DadaDeBaja',10);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (59,N'ELF-0005',N'Laboratorio de Biotecnología - Estante C',NULL,N'Modelo discontinuado; retirado del uso.',N'DadaDeBaja',10);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (60,N'ELF-0006',N'Laboratorio de Biotecnología - Estante C',NULL,N'Modelo discontinuado; retirado del uso.',N'DadaDeBaja',10);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (61,N'BAL-0001',N'Laboratorio de Química - Sala de balanzas',N'AX224-00001',NULL,N'Disponible',11);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (62,N'BAL-0002',N'Laboratorio de Química - Sala de balanzas',N'AX224-00002',NULL,N'Disponible',11);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (63,N'BAL-0003',N'Laboratorio de Química - Sala de balanzas',N'AX224-00003',NULL,N'Disponible',11);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (64,N'BAL-0004',N'Laboratorio de Química - Sala de balanzas',NULL,NULL,N'Disponible',11);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (65,N'BAL-0005',N'Laboratorio de Química - Sala de balanzas',N'AX224-00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',11);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (66,N'BAL-0006',N'Laboratorio de Química - Sala de balanzas',N'AX224-00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',11);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (67,N'PHM-0001',N'Laboratorio de Química - Mesada 2',N'PH700-00001',NULL,N'Disponible',12);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (68,N'PHM-0002',N'Laboratorio de Química - Mesada 2',N'PH700-00002',NULL,N'Disponible',12);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (69,N'PHM-0003',N'Laboratorio de Química - Mesada 2',N'PH700-00003',NULL,N'Disponible',12);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (70,N'PHM-0004',N'Laboratorio de Química - Mesada 2',NULL,NULL,N'Disponible',12);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (71,N'PHM-0005',N'Laboratorio de Química - Mesada 2',N'PH700-00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',12);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (72,N'PHM-0006',N'Laboratorio de Química - Mesada 2',N'PH700-00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',12);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (73,N'AGI-0001',N'Laboratorio de Química - Mesada 1',N'HS7-00001',NULL,N'Disponible',13);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (74,N'AGI-0002',N'Laboratorio de Química - Mesada 1',N'HS7-00002',NULL,N'Disponible',13);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (75,N'AGI-0003',N'Laboratorio de Química - Mesada 1',N'HS7-00003',NULL,N'Disponible',13);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (76,N'AGI-0004',N'Laboratorio de Química - Mesada 1',NULL,NULL,N'Disponible',13);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (77,N'AGI-0005',N'Laboratorio de Química - Mesada 1',N'HS7-00005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',13);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (78,N'AGI-0006',N'Laboratorio de Química - Mesada 1',N'HS7-00006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',13);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (79,N'ESP-0001',N'Laboratorio de Química - Sala de instrumental',N'UV190000001',NULL,N'Disponible',14);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (80,N'ESP-0002',N'Laboratorio de Química - Sala de instrumental',N'UV190000002',NULL,N'Disponible',14);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (81,N'ESP-0003',N'Laboratorio de Química - Sala de instrumental',N'UV190000003',NULL,N'Disponible',14);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (82,N'ESP-0004',N'Laboratorio de Química - Sala de instrumental',NULL,NULL,N'Disponible',14);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (83,N'ESP-0005',N'Laboratorio de Química - Sala de instrumental',N'UV190000005',N'En revisión técnica; vuelve en la próxima semana.',N'EnMantenimiento',14);
INSERT INTO UnidadesFisicas (Id,IdInstitucional,UbiFisica,NumSerie,Observaciones,Estado,TipoEquipamientoId) VALUES (84,N'ESP-0006',N'Laboratorio de Química - Sala de instrumental',N'UV190000006',N'Daño irreparable; dada de baja por el centro de equipamiento.',N'DadaDeBaja',14);
SET IDENTITY_INSERT [UnidadesFisicas] OFF;

-- TiposDeUsuario (6 filas)
SET IDENTITY_INSERT [TiposDeUsuario] ON;
INSERT INTO TiposDeUsuario (Id,Permisos,Nombre,Descripcion,Imagen,MaxUnidadesPorCatalogo,MaxPrestamosConcurrentes) VALUES (1,N'',N'Sin permisos',N'Cuenta registrada que espera que un administrador le asigne un tipo de usuario.',N'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABi0lEQVR42u2b0U0EMQxEk9H2BkVABVAOVABFcNXBFxI60LGb2OPxxv48ndZ+Y2eVeJ3WysrKVrbOdPby9vG597/Pj/c9vQBHgKME6erg3kL0LOBeQvRs4NZCIDO8hX9khreIo2cGt1gSOBP8SHw4E/xInFtUkE8Pd79+e32/6G6FrbL/F7iXEHveB1CDP/I/i7ihBs8WAVHrnSnCsABZ3vozVQDV7LOqAGfP/n88aIsbVsj+La6qAG8Hs7s67+1xVQBj/Y9m0SP713y0CjgKwzoZUpfAXijmsZjeD/iGU+kHhDVEImAlBGA2RGQEmOkFeIuxKZ8Efz7DSwgowzOOxcgA7/lcZIH3ej4sv7Yyenizfq75kCXzXv6QEd7SL7LCW/lH5IRWE/h0jszZt4gD0XN60YMT1RIb/bysUv574rnFURWgMrMbNTQFpcHliIkxqE1vs8fltuw9PepWOEsVHIkTKnP7UZOi/QyzwjOJgeItDmYcUL3KwvJfN0Za3RlqdWusLXxvsKxscfsCzJLHh9CjvCgAAAAASUVORK5CYII=',0,0);
INSERT INTO TiposDeUsuario (Id,Permisos,Nombre,Descripcion,Imagen,MaxUnidadesPorCatalogo,MaxPrestamosConcurrentes) VALUES (2,N'ConsultarCatalogo,ConsultarDisponibilidad,SolicitarPrestamo,ConsultarMisSolicitudes',N'Estudiante',N'Estudiantes de la facultad: consultan el catálogo y solicitan préstamos de equipamiento.',N'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABiklEQVR42u2bQU7EMAxFk69u4EpwNVjA1eBKzBJWSGhAQ5vY39+NvRyNar9vp0pcp7WysrKVrTOdPbx8fO797/vzfU8vwBHgKEG6Ori3ED0LuJcQPRu4tRDIDG/hH5nhLeLomcEtlgTOBD8SH84EPxLnFhXk29Pdr98eXy+6W2Gr7P8F7iXEnvcB1OCP/M8ibqjBs0VA1HpnijAsQJa3/kwVQDX7rCrA2bP/Hw/a4oYVsn+LqyrA28Hsrs57e1wVwFj/o1n0yP41H60CjsKwTobUJbAXinkspvcDvuFU+gFhDZEIWAkBmA0RGQFmegHeYmzKJ8Gfz/ASAsrwjGMxMsB7PhdZ4L2eD8uvrYwe3qyfaz5kybyXP2SEt/SLrPBW/hE5odUEPp0jc/Yt4kD0nF704ES1xEY/L6uU/554bnFUBajM7EYNTUFpcDliYgxq09vscbkte0+PuhXOUgVH4oTK3H7UpGg/w6zwTGKgeIuDGQdUr7Kw/NeNkVZ3hlrdGmsL3xssK1vcvgDTb8v7Luk3MwAAAABJRU5ErkJggg==',2,2);
INSERT INTO TiposDeUsuario (Id,Permisos,Nombre,Descripcion,Imagen,MaxUnidadesPorCatalogo,MaxPrestamosConcurrentes) VALUES (3,N'ConsultarCatalogo,ConsultarHistorico,ConsultarDisponibilidad,SolicitarPrestamo,ConsultarMisSolicitudes',N'Docente',N'Docentes de la facultad: solicitan préstamos con topes más altos y consultan el histórico.',N'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABiklEQVR42u2bUU7EMAxEk1EPAJeCW4HgVnAp9gbwhYQWtLSJPR439udqVfuNnSpxndbKyspWts50dvf28rn3v5fH555egCPAUYJ0dXBvIXoWcC8hejZwayGQGd7CPzLDW8TRM4NbLAmcCX4kPpwJfiTOLSrIj4enX7/dv7/qboWtsv8XuJcQe94HUIM/8j+LuKEGzxYBUeudKcKwAFne+jNVANXss6oAZ8/+fzxoixtWyP4trqoAbwezuzrv7XFVAGP9j2bRI/vXfLQKOArDOhlSl8BeKOaxmN4P+IZT6QeENUQiYCUEYDZEZASY6QV4i7EpnwR/PsNLCCjDM47FyADv+Vxkgfd6Piy/tjJ6eLN+rvmQJfNe/pAR3tIvssJb+UfkhFYT+HSOzNm3iAPRc3rRgxPVEhv9vKxS/nviucVRFaAysxs1NAWlweWIiTGoTW+zx+W27D096lY4SxUciRMqc/tRk6L9DLPCM4mB4i0OZhxQvcrC8l83RlrdGWp1a6wtfG+wrGxx+wLGV9PGmW25PwAAAABJRU5ErkJggg==',40,3);
INSERT INTO TiposDeUsuario (Id,Permisos,Nombre,Descripcion,Imagen,MaxUnidadesPorCatalogo,MaxPrestamosConcurrentes) VALUES (4,N'ConsultarCatalogo,GestionarUnidades,RealizarPrestamo,RegistrarDevolucion',N'Funcionario del centro de equipamiento',N'Personal del centro: gestiona unidades, entrega préstamos y registra devoluciones.',N'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABh0lEQVR42u2bwU7EMAxEk1FPLD8JHwY/yXJlT0hoQUub2ONxYx9Xq9pv7FSJ67RWVla2snWms+vb5Wvvfy+v155egCPAUYJ0dXBvIXoWcC8hejZwayGQGd7CPzLDW8TRM4NbLAmcCX4kPpwJfiTOLSrIp5ePX799vj/rboWtsv8XuJcQe94HUIM/8j+LuKEGzxYBUeudKcKwAFne+jNVANXss6oAZ8/+fzxoixtWyP4jrqoAbwezuzrv7XFVAGP9j2bRI/v3fLQKOArDOhlSl8BeKOaxmN4P+IZT6QeENUQiYCUEYDZEZASY6QV4i7EpnwR/PsNLCCjDM47FyADv+Vxkgfd6Piy/tjJ6eLN+7vmQJfNe/pAR3tIvssJb+UfkhFYT+HSOzNm3iAPRc3rRgxPVEhv9vKxS/nviecRRFaAysxs1NAWlweWIiTGoTW+zx+W27D096lY4SxUciRMqc/tRk6L9DLPCM4mB4i0OZhxQvcrC8l83RlrdGWp1a6wtfG+wrGxxuwHl282KHT/3UQAAAABJRU5ErkJggg==',0,0);
INSERT INTO TiposDeUsuario (Id,Permisos,Nombre,Descripcion,Imagen,MaxUnidadesPorCatalogo,MaxPrestamosConcurrentes) VALUES (5,N'ConsultarCatalogo,ConsultarHistorico,GestionarUnidades,RegistrarDevolucion,GestionarCatalogo,EvaluarSolicitud,ConsultarPrestamos',N'Supervisor',N'Gestiona el catálogo y las unidades, evalúa solicitudes y consulta préstamos e histórico.',N'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABiklEQVR42u2bQU7EMAxFk69u4IxwCzgF3ALOyCxhhYQGNLSJ/f3d2MvRqPb7dqrEdVorKytb2TrT2cvDx+fe/z6/3/f0AhwBjhKkq4N7C9GzgHsJ0bOBWwuBzPAW/pEZ3iKOnhncYkngTPAj8eFM8CNxblFBPr3d/frt9fGiuxW2yv5f4F5C7HkfQA3+yP8s4oYaPFsERK13pgjDAmR5689UAVSzz6oCnD37//GgLW5YIfu3uKoCvB3M7uq8t8dVAYz1P5pFj+xf89Eq4CgM62RIXQJ7oZjHYno/4BtOpR8Q1hCJgJUQgNkQkRFgphfgLcamfBL8+QwvIaAMzzgWIwO853ORBd7r+bD82sro4c36ueZDlsx7+UNGeEu/yApv5R+RE1pN4NM5MmffIg5Ez+lFD05US2z087JK+e+J5xZHVYDKzG7U0BSUBpcjJsagNr3NHpfbsvf0qFvhLFVwJE6ozO1HTYr2M8wKzyQGirc4mHFA9SoLy3/dGGl1Z6jVrbG28L3BsrLF7QtqKcjdsAOVfgAAAABJRU5ErkJggg==',0,0);
INSERT INTO TiposDeUsuario (Id,Permisos,Nombre,Descripcion,Imagen,MaxUnidadesPorCatalogo,MaxPrestamosConcurrentes) VALUES (6,N'ConsultarCatalogo,ConsultarHistorico,ConsultarDisponibilidad,SolicitarPrestamo,ConsultarMisSolicitudes,GestionarUnidades,RealizarPrestamo,RegistrarDevolucion,GestionarCatalogo,EvaluarSolicitud,ConsultarPrestamos,GestionarUsuarios',N'Administrador',N'Gestiona usuarios y tipos de usuario; tiene todos los permisos y no tiene límites.',N'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABh0lEQVR42u2b0W3DMAxEpYMHaHdKpk52ajdovwoEaZHaEnk8WuRnEJh8R8qQaKq1srKyla0znX1cLl97//t+v/f0AhwBjhKkq4N7C9GzgHsJ0bOBWwuBzPAW/pEZ3iKOnhncYkngTPAj8eFM8CNxblFBvt1uv377vF51t8JW2f8L3EuIPe8DqMEf+Z9F3FCDZ4uAqPXOFGFYgCxv/ZkqgGr2WVWAs2f/Px60xQ0rZP8VV1WAt4PZXZ339rgqgLH+R7Pokf1nPloFHIVhnQypS2AvFPNYTO8H/MCp9APCGiIRsBICMBsiMgLM9AK8xdiUT4KPz/ASAsrwjGMxMsB7PhdZ4L2eD8uvrYwe3qyfZz5kybyXP2SEt/SLrPBW/hE5odUEPp0jc/Yt4kD0nF704ES1xEY/L6uU/554XnFUBajM7EYNTUFpcDliYgxq09vscbkte0+PuhXOUgVH4oTK3H7UpGg/w6zwTGKgeIuDGQdUr7Kw/NeNkVZ3hlrdGmsL3xssK1vcvgG0hdBvKvPwGAAAAABJRU5ErkJggg==',NULL,NULL);
SET IDENTITY_INSERT [TiposDeUsuario] OFF;

-- Usuarios (9 filas)
SET IDENTITY_INSERT [Usuarios] ON;
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (1,N'11111111',N'Root',N'Admin',N'root@ucd.edu.uy',N'Root2026',N'Activo',0,6,0);
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (2,N'40000080',N'Carolina',N'Suárez',N'carolina.suarez@ucd.edu.uy',N'Prueba2026',N'Activo',0,6,0);
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (3,N'40000068',N'Diego',N'Rodríguez',N'diego.rodriguez@ucd.edu.uy',N'Prueba2026',N'Activo',0,2,0);
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (4,N'40000018',N'Sofía',N'Pereira',N'sofia.pereira@ucd.edu.uy',N'Prueba2026',N'Activo',0,5,0);
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (5,N'40000024',N'Martín',N'Gómez',N'martin.gomez@ucd.edu.uy',N'Prueba2026',N'Activo',0,4,0);
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (6,N'40000030',N'Lucía',N'Fernández',N'lucia.fernandez@ucd.edu.uy',N'Prueba2026',N'Activo',0,2,0);
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (7,N'40000046',N'Ana',N'Martínez',N'ana.martinez@ucd.edu.uy',N'Prueba2026',N'Activo',0,3,0);
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (8,N'40000096',N'Jorge',N'Benitez',N'jorge.benitez@ucd.edu.uy',N'Prueba2026',N'Activo',0,3,0);
INSERT INTO Usuarios (Id,Dni,Nombre,Apellido,Correo,Contrasena,Estado,DebeCambiarContrasena,TipoUsuarioId,EstaEliminado) VALUES (9,N'40000074',N'Pedro',N'Silva',N'pedro.silva@ucd.edu.uy',N'Ucd12345',N'Desactivado',1,2,0);
SET IDENTITY_INSERT [Usuarios] OFF;

-- Prestamos (15 filas)
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'D24C9333-ED2B-45FA-BCF6-10AC3C1E68D9',N'Devuelto',8,12,1,'2026-11-11 23:59:14.7211349','2026-11-20 00:00:00.0000000',NULL,NULL);
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'50D8A84B-DEEB-43DD-8F0A-1197549ABA0B',N'Devuelto',6,3,1,'2026-11-12 00:08:36.6437410','2026-11-19 00:00:00.0000000',NULL,NULL);
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'B9DF2FC2-9469-4F2F-ABA3-13C6F6F965F0',N'Rechazado',8,4,5,'2026-11-11 23:58:37.8651777','2026-11-16 00:00:00.0000000',NULL,N'No hay unidades suficientes para el tipo de equipamiento');
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'FE837C04-67FD-4691-A159-27331EE98AD3',N'Activo',3,2,2,'2026-11-11 23:52:07.4307892','2026-11-20 00:00:00.0000000',NULL,NULL);
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'5E1F4D6E-0B49-456B-A694-55257DB15FEC',N'Activo',6,3,2,'2026-11-12 00:20:37.5833873','2026-11-19 00:00:00.0000000',NULL,NULL);
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'D1D2B5BE-2B37-41BC-BDC2-58259226C473',N'Rechazado',8,7,4,'2026-11-11 23:58:52.4640819','2026-11-16 00:00:00.0000000',NULL,N'No hay unidades suficientes para el tipo de equipamiento');
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'B9327D38-259D-4712-B566-6B70A8C21C3A',N'Rechazado',6,7,1,'2026-11-11 23:51:43.1127107','2026-11-16 00:00:00.0000000',4,N'La centrífuga está reservada para las prácticas de Biotecnología de esta semana.');
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'9CA7A46A-120C-4D0C-829E-900D0AAB7571',N'Devuelto',6,1,1,'2026-11-12 00:08:24.3419363','2026-11-19 00:00:00.0000000',4,NULL);
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'65DA728E-1513-4AA7-82D4-A2CA0234E0D6',N'Rechazado',6,1,1,'2026-11-12 00:20:19.8107755','2026-11-19 00:00:00.0000000',NULL,N'Supera el maximo de unidades por catalogo al momento de la aprobacion');
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'6E47BB04-5129-4016-AB4B-A9D2CF61707E',N'Solicitado',6,7,1,'2026-11-12 00:21:49.7643759','2026-11-17 00:00:00.0000000',NULL,NULL);
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'565FF824-BC1C-4C1B-BF3A-BAB13AA25605',N'Rechazado',8,14,1,'2026-11-11 23:59:08.3053839','2026-11-16 00:00:00.0000000',4,N'El espectrofotómetro está en calibración hasta fin de mes.');
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'C2D10471-D8C5-4DDA-8CD6-CD77BC1DB3F4',N'Rechazado',7,5,1,'2026-11-11 23:57:30.8957235','2026-11-20 00:00:00.0000000',4,N'Las laptops quedan reservadas para el curso de simulación de circuitos de esta semana.');
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'3D53E267-5AAB-41DB-BFDA-F2F9B1A4155C',N'Devuelto',6,2,1,'2026-11-11 23:51:35.7171720','2026-11-20 00:00:00.0000000',NULL,NULL);
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'E8AB578A-66F2-45A0-BA7D-F76EF5A05285',N'Activo',7,7,1,'2026-11-11 23:57:08.6700067','2026-11-11 00:00:00.0000000',4,NULL);
INSERT INTO Prestamos (Id,Estado,SolicitanteId,TipoEquipamientoId,CantidadSolicitada,FechaHoraPrestamo,FechaEsperadaDevolucion,EvaluadorId,MotivoRechazo) VALUES (N'E2855EE2-A3BA-4E41-BD73-FD16F972D864',N'Activo',7,1,1,'2026-11-11 23:57:16.8260506','2026-11-18 00:00:00.0000000',4,NULL);

-- UnidadesAsignadas (10 filas)
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'E2855EE2-A3BA-4E41-BD73-FD16F972D864',1);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'9CA7A46A-120C-4D0C-829E-900D0AAB7571',2);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'3D53E267-5AAB-41DB-BFDA-F2F9B1A4155C',7);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'FE837C04-67FD-4691-A159-27331EE98AD3',8);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'FE837C04-67FD-4691-A159-27331EE98AD3',9);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'50D8A84B-DEEB-43DD-8F0A-1197549ABA0B',13);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'5E1F4D6E-0B49-456B-A694-55257DB15FEC',13);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'5E1F4D6E-0B49-456B-A694-55257DB15FEC',14);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'E8AB578A-66F2-45A0-BA7D-F76EF5A05285',37);
INSERT INTO UnidadesAsignadas (PrestamoId,UnidadFisicaId) VALUES (N'D24C9333-ED2B-45FA-BCF6-10AC3C1E68D9',67);

-- Movimientos (38 filas)
SET IDENTITY_INSERT [Movimientos] ON;
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (1,N'3D53E267-5AAB-41DB-BFDA-F2F9B1A4155C',N'Solicitud',NULL,NULL,6,'2026-11-11 23:51:35.7171720');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (2,N'3D53E267-5AAB-41DB-BFDA-F2F9B1A4155C',N'Prestamo',NULL,7,NULL,'2026-11-11 23:51:35.7171720');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (3,N'B9327D38-259D-4712-B566-6B70A8C21C3A',N'Solicitud',NULL,NULL,6,'2026-11-11 23:51:43.1127107');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (4,N'FE837C04-67FD-4691-A159-27331EE98AD3',N'Solicitud',NULL,NULL,3,'2026-11-11 23:52:07.4307892');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (5,N'FE837C04-67FD-4691-A159-27331EE98AD3',N'Prestamo',NULL,8,NULL,'2026-11-11 23:52:07.4307892');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (6,N'FE837C04-67FD-4691-A159-27331EE98AD3',N'Prestamo',NULL,9,NULL,'2026-11-11 23:52:07.4307892');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (7,N'E8AB578A-66F2-45A0-BA7D-F76EF5A05285',N'Solicitud',NULL,NULL,7,'2026-11-11 23:57:08.6700067');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (8,N'E2855EE2-A3BA-4E41-BD73-FD16F972D864',N'Solicitud',NULL,NULL,7,'2026-11-11 23:57:16.8260506');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (9,N'C2D10471-D8C5-4DDA-8CD6-CD77BC1DB3F4',N'Solicitud',NULL,NULL,7,'2026-11-11 23:57:30.8957235');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (10,N'B9DF2FC2-9469-4F2F-ABA3-13C6F6F965F0',N'Solicitud',NULL,NULL,8,'2026-11-11 23:58:37.8651777');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (11,N'B9DF2FC2-9469-4F2F-ABA3-13C6F6F965F0',N'Rechazo',NULL,NULL,NULL,'2026-11-11 23:58:37.8651777');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (12,N'D1D2B5BE-2B37-41BC-BDC2-58259226C473',N'Solicitud',NULL,NULL,8,'2026-11-11 23:58:52.4640819');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (13,N'565FF824-BC1C-4C1B-BF3A-BAB13AA25605',N'Solicitud',NULL,NULL,8,'2026-11-11 23:59:08.3053839');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (14,N'D24C9333-ED2B-45FA-BCF6-10AC3C1E68D9',N'Solicitud',NULL,NULL,8,'2026-11-11 23:59:14.7211349');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (15,N'D24C9333-ED2B-45FA-BCF6-10AC3C1E68D9',N'Prestamo',NULL,67,NULL,'2026-11-11 23:59:14.7211349');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (16,N'565FF824-BC1C-4C1B-BF3A-BAB13AA25605',N'Rechazo',4,NULL,4,'2026-11-12 00:00:50.8512373');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (17,N'C2D10471-D8C5-4DDA-8CD6-CD77BC1DB3F4',N'Rechazo',4,NULL,4,'2026-11-12 00:01:17.9731352');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (18,N'E2855EE2-A3BA-4E41-BD73-FD16F972D864',N'Aprobacion',4,1,4,'2026-11-12 00:01:23.6208644');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (19,N'E2855EE2-A3BA-4E41-BD73-FD16F972D864',N'Prestamo',4,1,4,'2026-11-12 00:01:23.6208644');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (20,N'E8AB578A-66F2-45A0-BA7D-F76EF5A05285',N'Aprobacion',4,37,4,'2026-11-12 00:01:45.9939472');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (21,N'E8AB578A-66F2-45A0-BA7D-F76EF5A05285',N'Prestamo',4,37,4,'2026-11-12 00:01:45.9939472');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (22,N'D1D2B5BE-2B37-41BC-BDC2-58259226C473',N'Rechazo',NULL,NULL,NULL,'2026-11-12 00:01:47.7141267');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (23,N'B9327D38-259D-4712-B566-6B70A8C21C3A',N'Rechazo',4,NULL,4,'2026-11-12 00:02:02.8704837');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (24,N'3D53E267-5AAB-41DB-BFDA-F2F9B1A4155C',N'Devolucion',NULL,7,5,'2026-11-12 00:03:07.4978785');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (25,N'D24C9333-ED2B-45FA-BCF6-10AC3C1E68D9',N'Devolucion',NULL,67,5,'2026-11-12 00:03:15.0362287');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (26,N'9CA7A46A-120C-4D0C-829E-900D0AAB7571',N'Solicitud',NULL,NULL,6,'2026-11-12 00:08:24.3419363');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (27,N'50D8A84B-DEEB-43DD-8F0A-1197549ABA0B',N'Solicitud',NULL,NULL,6,'2026-11-12 00:08:36.6437410');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (28,N'50D8A84B-DEEB-43DD-8F0A-1197549ABA0B',N'Prestamo',NULL,13,NULL,'2026-11-12 00:08:36.6437410');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (29,N'9CA7A46A-120C-4D0C-829E-900D0AAB7571',N'Aprobacion',4,2,4,'2026-11-12 00:09:03.6028436');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (30,N'9CA7A46A-120C-4D0C-829E-900D0AAB7571',N'Prestamo',4,2,4,'2026-11-12 00:09:03.6028436');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (31,N'9CA7A46A-120C-4D0C-829E-900D0AAB7571',N'Devolucion',4,2,5,'2026-11-12 00:19:54.6291069');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (32,N'50D8A84B-DEEB-43DD-8F0A-1197549ABA0B',N'Devolucion',NULL,13,5,'2026-11-12 00:19:57.6456343');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (33,N'65DA728E-1513-4AA7-82D4-A2CA0234E0D6',N'Solicitud',NULL,NULL,6,'2026-11-12 00:20:19.8107755');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (34,N'5E1F4D6E-0B49-456B-A694-55257DB15FEC',N'Solicitud',NULL,NULL,6,'2026-11-12 00:20:37.5833873');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (35,N'5E1F4D6E-0B49-456B-A694-55257DB15FEC',N'Prestamo',NULL,13,NULL,'2026-11-12 00:20:37.5833873');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (36,N'5E1F4D6E-0B49-456B-A694-55257DB15FEC',N'Prestamo',NULL,14,NULL,'2026-11-12 00:20:37.5833873');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (37,N'65DA728E-1513-4AA7-82D4-A2CA0234E0D6',N'Rechazo',NULL,NULL,NULL,'2026-11-12 00:21:25.6382025');
INSERT INTO Movimientos (Id,PrestamoId,Tipo,EvaluadorId,UnidadId,ResponsableId,FechaHora) VALUES (38,N'6E47BB04-5129-4016-AB4B-A9D2CF61707E',N'Solicitud',NULL,NULL,6,'2026-11-12 00:21:49.7643759');
SET IDENTITY_INSERT [Movimientos] OFF;

-- Devoluciones (4 filas)
SET IDENTITY_INSERT [Devoluciones] ON;
INSERT INTO Devoluciones (Id,PrestamoId,RegistradaPorId,FechaHora) VALUES (1,N'3D53E267-5AAB-41DB-BFDA-F2F9B1A4155C',5,'2026-11-12 00:03:07.4978785');
INSERT INTO Devoluciones (Id,PrestamoId,RegistradaPorId,FechaHora) VALUES (2,N'D24C9333-ED2B-45FA-BCF6-10AC3C1E68D9',5,'2026-11-12 00:03:15.0362287');
INSERT INTO Devoluciones (Id,PrestamoId,RegistradaPorId,FechaHora) VALUES (3,N'9CA7A46A-120C-4D0C-829E-900D0AAB7571',5,'2026-11-12 00:19:54.6291069');
INSERT INTO Devoluciones (Id,PrestamoId,RegistradaPorId,FechaHora) VALUES (4,N'50D8A84B-DEEB-43DD-8F0A-1197549ABA0B',5,'2026-11-12 00:19:57.6456343');
SET IDENTITY_INSERT [Devoluciones] OFF;

-- UnidadesDevueltas (4 filas)
SET IDENTITY_INSERT [UnidadesDevueltas] ON;
INSERT INTO UnidadesDevueltas (Id,UnidadFisicaId,Condicion,Observaciones,DevolucionId) VALUES (1,7,N'SinObservaciones',NULL,1);
INSERT INTO UnidadesDevueltas (Id,UnidadFisicaId,Condicion,Observaciones,DevolucionId) VALUES (2,67,N'SinObservaciones',NULL,2);
INSERT INTO UnidadesDevueltas (Id,UnidadFisicaId,Condicion,Observaciones,DevolucionId) VALUES (3,2,N'SinObservaciones',NULL,3);
INSERT INTO UnidadesDevueltas (Id,UnidadFisicaId,Condicion,Observaciones,DevolucionId) VALUES (4,13,N'SinObservaciones',NULL,4);
SET IDENTITY_INSERT [UnidadesDevueltas] OFF;

-- RegistrosAuditoria (47 filas)
SET IDENTITY_INSERT [RegistrosAuditoria] ON;
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (1,N'AltaDeUsuario',N'Alta de Carolina Suárez (DNI 40000080) como Administrador',1,'2026-11-11 23:38:45.9062329');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (2,N'RegistroDeUsuario',N'Se registró Diego Rodríguez (DNI 40000068)',3,'2026-11-11 23:39:23.3246692');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (3,N'AltaDeUsuario',N'Alta de Sofía Pereira (DNI 40000018) como Supervisor',2,'2026-11-11 23:40:49.0027650');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (4,N'AltaDeUsuario',N'Alta de Martín Gómez (DNI 40000024) como Funcionario del centro de equipamiento',2,'2026-11-11 23:41:17.4107196');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (5,N'AltaDeUsuario',N'Alta de Lucía Fernández (DNI 40000030) como Funcionario del centro de equipamiento',2,'2026-11-11 23:42:02.3662431');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (6,N'AltaDeUsuario',N'Alta de Ana Martínez (DNI 40000046) como Funcionario del centro de equipamiento',2,'2026-11-11 23:42:21.2151834');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (7,N'CambioDeTipoDeUsuario',N'Lucía Fernández (DNI 40000030) pasó de Funcionario del centro de equipamiento a Estudiante',2,'2026-11-11 23:44:02.0133609');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (8,N'CambioDeTipoDeUsuario',N'Ana Martínez (DNI 40000046) pasó de Funcionario del centro de equipamiento a Docente',2,'2026-11-11 23:44:09.5251736');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (9,N'AltaDeUsuario',N'Alta de Jorge Benitez (DNI 40000096) como Docente',2,'2026-11-11 23:44:19.1080385');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (10,N'AltaDeUsuario',N'Alta de Pedro Silva (DNI 40000074) como Estudiante',2,'2026-11-11 23:44:57.4584454');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (11,N'CambioDeTipoDeUsuario',N'Diego Rodríguez (DNI 40000068) pasó de Sin permisos a Estudiante',2,'2026-11-11 23:45:28.4265877');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (12,N'DesactivacionDeUsuario',N'Se desactivó la cuenta de Pedro Silva (DNI 40000074)',2,'2026-11-11 23:45:51.5389967');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (13,N'SolicitudDePrestamo',N'Solicitud de préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',6,'2026-11-11 23:51:35.7171720');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (14,N'ActivacionDePrestamo',N'Activación del préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',NULL,'2026-11-11 23:51:35.7171720');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (15,N'SolicitudDePrestamo',N'Solicitud de préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',6,'2026-11-11 23:51:43.1127107');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (16,N'SolicitudDePrestamo',N'Solicitud de préstamo de Diego Rodríguez (DNI 40000068). Unidades: 2',3,'2026-11-11 23:52:07.4307892');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (17,N'ActivacionDePrestamo',N'Activación del préstamo de Diego Rodríguez (DNI 40000068). Unidades: 2',NULL,'2026-11-11 23:52:07.4307892');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (18,N'SolicitudDePrestamo',N'Solicitud de préstamo de Ana Martínez (DNI 40000046). Unidades: 1',7,'2026-11-11 23:57:08.6700067');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (19,N'SolicitudDePrestamo',N'Solicitud de préstamo de Ana Martínez (DNI 40000046). Unidades: 1',7,'2026-11-11 23:57:16.8260506');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (20,N'SolicitudDePrestamo',N'Solicitud de préstamo de Ana Martínez (DNI 40000046). Unidades: 1',7,'2026-11-11 23:57:30.8957235');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (21,N'SolicitudDePrestamo',N'Solicitud de préstamo de Jorge Benitez (DNI 40000096). Unidades: 5',8,'2026-11-11 23:58:37.8651777');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (22,N'RechazoDePrestamo',N'Rechazo del préstamo de Jorge Benitez (DNI 40000096). Unidades: 5',NULL,'2026-11-11 23:58:37.8651777');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (23,N'SolicitudDePrestamo',N'Solicitud de préstamo de Jorge Benitez (DNI 40000096). Unidades: 4',8,'2026-11-11 23:58:52.4640819');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (24,N'SolicitudDePrestamo',N'Solicitud de préstamo de Jorge Benitez (DNI 40000096). Unidades: 1',8,'2026-11-11 23:59:08.3053839');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (25,N'SolicitudDePrestamo',N'Solicitud de préstamo de Jorge Benitez (DNI 40000096). Unidades: 1',8,'2026-11-11 23:59:14.7211349');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (26,N'ActivacionDePrestamo',N'Activación del préstamo de Jorge Benitez (DNI 40000096). Unidades: 1',NULL,'2026-11-11 23:59:14.7211349');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (27,N'RechazoDePrestamo',N'Rechazo del préstamo de Jorge Benitez (DNI 40000096). Unidades: 1',4,'2026-11-12 00:00:50.8512373');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (28,N'RechazoDePrestamo',N'Rechazo del préstamo de Ana Martínez (DNI 40000046). Unidades: 1',4,'2026-11-12 00:01:17.9731352');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (29,N'AprobacionDePrestamo',N'Aprobación del préstamo de Ana Martínez (DNI 40000046). Unidades: 1',4,'2026-11-12 00:01:23.6208644');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (30,N'AprobacionDePrestamo',N'Aprobación del préstamo de Ana Martínez (DNI 40000046). Unidades: 1',4,'2026-11-12 00:01:45.9939472');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (31,N'IntentoDeAprobacion',N'Intento de aprobación del préstamo de Jorge Benitez (DNI 40000096). Unidades: 4',4,'2026-11-12 00:01:47.7141267');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (32,N'RechazoDePrestamo',N'Rechazo del préstamo de Jorge Benitez (DNI 40000096). Unidades: 4',NULL,'2026-11-12 00:01:47.7141267');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (33,N'RechazoDePrestamo',N'Rechazo del préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',4,'2026-11-12 00:02:02.8704837');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (34,N'DevolucionDePrestamo',N'Devolución del préstamo de Lucía Fernández (DNI 40000030): 1 unidad',5,'2026-11-12 00:03:07.4978785');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (35,N'DevolucionDePrestamo',N'Devolución del préstamo de Jorge Benitez (DNI 40000096): 1 unidad',5,'2026-11-12 00:03:15.0362287');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (36,N'SolicitudDePrestamo',N'Solicitud de préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',6,'2026-11-12 00:08:24.3419363');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (37,N'SolicitudDePrestamo',N'Solicitud de préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',6,'2026-11-12 00:08:36.6437410');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (38,N'ActivacionDePrestamo',N'Activación del préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',NULL,'2026-11-12 00:08:36.6437410');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (39,N'AprobacionDePrestamo',N'Aprobación del préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',4,'2026-11-12 00:09:03.6028436');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (40,N'DevolucionDePrestamo',N'Devolución del préstamo de Lucía Fernández (DNI 40000030): 1 unidad',5,'2026-11-12 00:19:54.6291069');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (41,N'DevolucionDePrestamo',N'Devolución del préstamo de Lucía Fernández (DNI 40000030): 1 unidad',5,'2026-11-12 00:19:57.6456343');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (42,N'SolicitudDePrestamo',N'Solicitud de préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',6,'2026-11-12 00:20:19.8107755');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (43,N'SolicitudDePrestamo',N'Solicitud de préstamo de Lucía Fernández (DNI 40000030). Unidades: 2',6,'2026-11-12 00:20:37.5833873');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (44,N'ActivacionDePrestamo',N'Activación del préstamo de Lucía Fernández (DNI 40000030). Unidades: 2',NULL,'2026-11-12 00:20:37.5833873');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (45,N'IntentoDeAprobacion',N'Intento de aprobación del préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',4,'2026-11-12 00:21:25.6382025');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (46,N'RechazoDePrestamo',N'Rechazo del préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',NULL,'2026-11-12 00:21:25.6382025');
INSERT INTO RegistrosAuditoria (Id,Accion,Detalle,ResponsableId,FechaHora) VALUES (47,N'SolicitudDePrestamo',N'Solicitud de préstamo de Lucía Fernández (DNI 40000030). Unidades: 1',6,'2026-11-12 00:21:49.7643759');
SET IDENTITY_INSERT [RegistrosAuditoria] OFF;

-- Notificaciones (8 filas)
SET IDENTITY_INSERT [Notificaciones] ON;
INSERT INTO Notificaciones (Id,PrestamoId,Tipo,FechaHora,EstaLeida) VALUES (1,N'565FF824-BC1C-4C1B-BF3A-BAB13AA25605',N'SolicitudRechazada','2026-11-12 00:00:50.8512373',0);
INSERT INTO Notificaciones (Id,PrestamoId,Tipo,FechaHora,EstaLeida) VALUES (2,N'C2D10471-D8C5-4DDA-8CD6-CD77BC1DB3F4',N'SolicitudRechazada','2026-11-12 00:01:17.9731352',0);
INSERT INTO Notificaciones (Id,PrestamoId,Tipo,FechaHora,EstaLeida) VALUES (3,N'E2855EE2-A3BA-4E41-BD73-FD16F972D864',N'SolicitudAprobada','2026-11-12 00:01:23.6208644',0);
INSERT INTO Notificaciones (Id,PrestamoId,Tipo,FechaHora,EstaLeida) VALUES (4,N'E8AB578A-66F2-45A0-BA7D-F76EF5A05285',N'SolicitudAprobada','2026-11-12 00:01:45.9939472',0);
INSERT INTO Notificaciones (Id,PrestamoId,Tipo,FechaHora,EstaLeida) VALUES (5,N'D1D2B5BE-2B37-41BC-BDC2-58259226C473',N'SolicitudRechazada','2026-11-12 00:01:47.7141267',0);
INSERT INTO Notificaciones (Id,PrestamoId,Tipo,FechaHora,EstaLeida) VALUES (6,N'B9327D38-259D-4712-B566-6B70A8C21C3A',N'SolicitudRechazada','2026-11-12 00:02:02.8704837',0);
INSERT INTO Notificaciones (Id,PrestamoId,Tipo,FechaHora,EstaLeida) VALUES (7,N'9CA7A46A-120C-4D0C-829E-900D0AAB7571',N'SolicitudAprobada','2026-11-12 00:09:03.6028436',0);
INSERT INTO Notificaciones (Id,PrestamoId,Tipo,FechaHora,EstaLeida) VALUES (8,N'65DA728E-1513-4AA7-82D4-A2CA0234E0D6',N'SolicitudRechazada','2026-11-12 00:21:25.6382025',0);
SET IDENTITY_INSERT [Notificaciones] OFF;

COMMIT;
