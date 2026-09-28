---
name: csharp-code-quality-review
description: Revisa clases C#, tests MSTest y componentes Razor contra los lineamientos de Clean Code (capítulos 1-10 y 12) y las convenciones de codificación de C# de Microsoft. Usar cuando el usuario pida revisar calidad de código, chequear Clean Code, o antes de commitear una clase nueva o modificada del proyecto UCD (Obligatorio 1, DA1).
---

# Revisión de calidad de código C# (Clean Code + convenciones .NET)

Cubre los capítulos de Clean Code que pide la letra: 1 al 10 y 12.

## Restricciones importantes — leer primero

**Solo lectura.** Esta skill revisa y reporta: no edites, crees ni borres archivos,
no ejecutes comandos que cambien el repositorio y no hagas commits. Los cambios los
decide y aplica el equipo después de leer el reporte.

### Funcionalidades manuales

Este proyecto tiene 3 funcionalidades de desarrollo manual sin asistencia de IA:
**Registro en el sistema, Consultas al catálogo, y Préstamos**.

Antes de revisar cualquier archivo, preguntá al usuario si el código pertenece a
alguna de esas 3 funcionalidades. Si la respuesta es sí, NO reescribas ni sugieras
código concreto — solo podés señalar en términos generales qué principio de Clean
Code podría estar en juego (ej. "este método parece tener más de una responsabilidad"),
sin mostrar cómo arreglarlo. Para el resto del proyecto (dominio de otras funcionalidades,
capa de datos, UI, etc.) no hay restricción y podés sugerir cambios concretos.

Si no podés preguntar (por ejemplo, en una revisión automática de un PR), deducilo
por la rama y los archivos: `feature/registro`, `Usuario`, `ServicioUsuario` o
`Registro.razor` son de Registro; lo mismo con catálogo y préstamos. Ante la duda,
tratalo como funcionalidad manual.

## Cómo revisar

Leé el/los archivo(s) que el usuario indique (o el diff de los últimos cambios si
pide "revisar lo último que hice"). Chequeá cada punto de la lista de abajo. Para
cada violación encontrada, indicá: archivo, línea aproximada, qué principio viola,
y por qué importa (no solo "está mal", explicar el motivo).

No hace falta que reportes cada punto que SÍ se cumple — enfocate en lo que falta
o está mal. Si el código está limpio, decilo brevemente y no inventes objeciones
menores solo para tener algo que decir.

### 1. Nombres (Clean Code cap. 2)
Fuente principal: diapositivas del curso "04.10e Clean Code 1" (G. Mousqués).
- Nombres que revelan intención: por qué existe, qué hace y cómo se usa
- Evitar la desinformación (ej. no llamar `lista` a algo que no es una lista)
- Distinciones significativas: nada de palabras de ruido como Info, Datos, Procesador,
  Sistema (ej. `ModificarInfo` → preferir `Modificar`)
- Nombres pronunciables y fáciles de buscar: sin abreviaturas (`UbicacionFisica`, no
  `UbiFisica`; `CantidadSolicitada`, no `CantSolicitada`)
- Largo según el alcance:
  - Variables locales de alcance chico pueden ser cortas; atributos y propiedades
    de clase, largos y significativos
  - Métodos públicos: cortos y significativos. Métodos privados o muy específicos:
    más largos y descriptivos
  - Clases públicas: nombres cortos (un par de palabras), sustantivos del dominio del
    cliente (`Prestamo`, `UnidadFisica`), nunca verbos
- Métodos: siempre incluyen un verbo (`Prestar`, no `Prestada`; `EnviarAMantenimiento`,
  no `AMantenimiento`). Para preguntas y accesores, la convención is/get/set
  (`EstaDisponible`)
- La misma palabra para métodos equivalentes en distintas clases (si `TipoEquipamiento`
  usa `Modificar`, las demás también), y nunca el mismo término para dos propósitos
- Evitar encodings: sin prefijos ni información de tipo o alcance en el nombre. Las
  interfaces con `I` se aceptan porque es la convención de C# (la diapositiva dice
  "si la convención del lenguaje lo requiere, respetarlo"). El prefijo `_` en campos
  privados está en duda: ver sección 11
- PascalCase para clases y métodos (`Prestamo`, `ModificarNombre`)
- Sin espacios, sin tildes, sin ñ (compatibilidad con C#, convención del proyecto)

### 2. Funciones (Clean Code cap. 3)
- Chicas — si un método no entra en una pantalla sin scrollear, sospechar
- Hacen UNA sola cosa (si el nombre necesita un "y" para describirlo, probablemente hace dos)
- Pocos argumentos (0-2 ideal, 3 aceptable, 4+ es señal de alerta — pensar en agrupar en un objeto)
- Sin efectos secundarios ocultos (un método `EstaActivo()` no debería, de paso, cambiar el estado)
- Un solo nivel de abstracción por función (no mezclar detalle de bajo nivel con lógica de alto nivel)

### 3. Comentarios (Clean Code cap. 4)
- Preferir código autoexplicativo antes que comentar código confuso
- Marcar comentarios que solo repiten lo que el código ya dice (`// suma uno` sobre `i++`)
- Comentarios de código muerto (código comentado en vez de borrado) → señalar para eliminar

### 4. Formato (Clean Code cap. 5)
- Indentación consistente
- Líneas en blanco separando conceptos distintos dentro de un método
- Orden lógico: campos, constructor, métodos públicos, métodos privados

### 5. Objetos y estructuras de datos (Clean Code cap. 6)
- Encapsulamiento: propiedades con `private set` cuando no deberían modificarse desde afuera
  (¿esa propiedad tiene un método propio para modificarse, tipo `ModificarNombre`, o se
  está exponiendo un `set` público que rompe esa regla? El diagrama de dominio no está en
  el repositorio en un formato legible: basate en el código y, si hay dudas, preguntá)
- Evitar exponer estructura interna innecesariamente

### 6. Manejo de errores (Clean Code cap. 7)
- Excepciones en vez de códigos de error o valores mágicos de retorno
- Mensajes de excepción descriptivos (no genéricos tipo "Error")
- Las reglas del dominio lanzan `DominioException`, no `Exception` ni `ArgumentException`
- Mensajes de excepción sin tildes ni ñ (convención del equipo, ej. "El correo no puede estar vacio")
- No tragarse excepciones con catch vacíos

### 7. Límites (Clean Code cap. 8)
- El código propio no depende directamente de detalles de terceros o de otras capas
  (ej. la UI no debería conocer cómo guarda los datos el repositorio)
- Las dependencias entre proyectos van en un solo sentido: UI → Servicios → Dominio,
  y Servicios → Data. Señalar cualquier referencia al revés (ej. un proyecto de
  producción que referencia un proyecto de tests)
- Interfaces en los bordes entre capas cuando ayudan a desacoplar (ej. repositorios)

### 8. Tests unitarios (Clean Code cap. 9)
Aplicar a los archivos de `*.Tests`:
- Un concepto por test: si un test verifica dos comportamientos distintos, dividirlo
- Nombre descriptivo con el patrón que ya usa el proyecto:
  `Accion_Escenario_ResultadoEsperado` (ej. `Constructor_FechaValida_GuardaLaFecha`),
  con el mismo vocabulario en los tests de un mismo tema
- Estructura Arrange / Act / Assert clara, separada por líneas en blanco
- F.I.R.S.T.: rápidos, independientes entre sí (sin estado compartido que dependa
  del orden), repetibles (sin `DateTime.Now` ni datos aleatorios sin controlar),
  autovalidados (un Assert que decide, sin revisar la consola) y escritos antes
  del código (TDD)
- Tests limpios como código de producción: sin duplicación, usar métodos de ayuda
  para crear objetos (ej. `CrearDevolucion(...)`) en vez de repetir constructores.
  Esos helpers se llaman con argumentos con nombre (`CrearUsuario(correo: "...")`):
  un argumento posicional puede cargar el valor en el parámetro equivocado
- Casos de una misma clase de equivalencia agrupados con `[DataRow]` en un solo
  `[TestMethod]`, en vez de un test copiado por cada valor
- Además de los tests que esperan excepción, al menos un test de control con datos
  válidos por cada regla
- Para excepciones, verificar el tipo concreto (`DominioException`) y, si aporta,
  el mensaje
- Bordes cubiertos: valores límite (ej. 2 y 50 caracteres, y también 1 y 51),
  vacío, null y espacios en blanco

### 9. Clases (Clean Code cap. 10)
- Clases chicas con responsabilidad única (¿esta clase hace más de una cosa? ¿debería dividirse?)
- Alta cohesión (los métodos de la clase usan la mayoría de sus atributos)
- Validaciones agrupadas en métodos privados con nombres claros (ej. `ValidarNombre`)

### 10. Diseño emergente (Clean Code cap. 12)
Las cuatro reglas del diseño simple, en orden de prioridad:
1. Pasa todos los tests
2. No tiene duplicación (misma validación o constante repetida en varias clases → señalar)
3. Expresa la intención del programador (nombres, constantes con nombre en vez de
   números mágicos como `80` o `300`)
4. Minimiza la cantidad de clases y métodos (no crear abstracciones que todavía
   no se necesitan)

### 11. Convenciones de C# (Microsoft, linkeadas en la letra)
- PascalCase: clases, métodos, propiedades, eventos, namespaces
- camelCase: variables locales, parámetros
- Campos privados: la convención de Microsoft usa `_camelCase`, pero la diapositiva de
  "encodings" del curso muestra `private string _name;` como ejemplo a evitar. Pendiente
  de confirmar en el foro. Mientras tanto, no marcarlo como error; solo señalar si el
  proyecto mezcla los dos estilos. El proyecto prefiere propiedades autoimplementadas
- `namespace` file-scoped (`namespace Dominio;`) — es el estilo que ya usa este proyecto,
  mantenerlo consistente en archivos nuevos
- Tipos de variables: Microsoft permite `var` cuando el tipo es obvio por el lado derecho,
  pero el proyecto usa tipos explícitos. No marcar los tipos explícitos como error; sí
  señalar un `var` cuando el tipo no se deduce de la línea, o si un archivo mezcla estilos

### 12. Componentes Razor (UI de Blazor)
Aplicar a los archivos `.razor` y `.razor.css`:
- Lógica de negocio fuera de la UI: las páginas llaman a servicios, no validan
  reglas del dominio por su cuenta (validaciones de formulario como "las
  contraseñas no coinciden" sí son de la UI)
- Bloque `@code` corto; si crece mucho, pasar a un archivo `.razor.cs` o a un componente hijo
- Markup repetido en varias páginas → extraer a un componente en `Components/Shared`
- Handlers con verbos (`Registrar`, `IniciarSesion`); para los campos privados ver sección 11
- Un método no puede llamarse igual que la página (ej. método `Ingresar` en `Ingresar.razor`)
- Estilos en el `.razor.css` de cada componente o en `app.css` usando las variables
  de color (`var(--navy)`), no colores sueltos
- Accesibilidad básica: cada input con su `<label for>`, mensajes de error con `role="alert"`

## Formato del reporte

Organizá los hallazgos por archivo. Para cada uno:

```
### NombreDelArchivo.cs

- **Línea X** — [categoría, ej: Nombres / Funciones / Manejo de errores] — severidad
  Qué está mal y por qué importa.
```

Severidad:
- **Alta**: puede causar un bug, rompe una regla del dominio o deja un caso sin test.
- **Media**: dificulta leer o mantener el código (nombres engañosos, métodos largos,
  duplicación, números mágicos).
- **Baja**: formato o estilo.

Ordená los hallazgos de cada archivo de mayor a menor severidad.

Al final, un resumen de 1-2 líneas con la impresión general (no una lista repetida).
