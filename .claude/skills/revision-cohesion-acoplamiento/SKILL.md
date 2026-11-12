---
name: revision-cohesion-acoplamiento
description: Analiza la cohesión y el acoplamiento de clases C# del proyecto UCD (DA1, Obligatorio 2) con los conceptos y patrones GRASP del curso, y propone mejoras concretas con su costo. Usar cuando se pida revisar cohesión, acoplamiento, dependencias, responsabilidades o GRASP de una clase o servicio, antes de abrir un PR, o al documentar el diseño de los servicios.
---

# Revisión de cohesión y acoplamiento (GRASP)

Complementa a `csharp-code-quality-review` (que mira Clean Code dentro de cada clase).
Esta skill mira **entre clases**: de quién depende cada una, quién depende de ella y si
sus responsabilidades están bien repartidas. Fuente: diapositivas del curso "06.10e GRASP"
y "04.00 Conceptos esenciales" (DA1, Cátedra de Ingeniería de Software).

## Restricciones — leer primero

1. **Primero se analiza, después se cambia.** La skill arranca en modo solo lectura y
   entrega el reporte (sección "Formato del reporte"). No edita archivos hasta que el
   usuario elija qué propuestas aplicar. Antes de cambiar código, pedir que todo esté
   commiteado (para poder volver con `git revert`).
2. **Partes manuales (sin IA).** En el Obligatorio 2 son manuales, con TDD, los
   repositorios con Entity Framework de **Usuario, Catálogo y Préstamo**: la clase del
   repositorio, sus tests y la configuración de EF de esa entidad
   (`IEntityTypeConfiguration`, `DbSet`, migración). Si el código a revisar es parte de
   eso, solo se nombra el principio en juego ("este repositorio conoce reglas del
   dominio"), sin mostrar cómo resolverlo ni escribir código. Ante la duda, preguntar.
3. **Decisiones del equipo que no se marcan como error** (sí se pueden mencionar como
   alternativa, con su costo):
   - Los servicios dependen de los repositorios concretos, sin interfaces (arquitectura
     simplificada del curso). Proponer una interfaz solo si hay una variación concreta
     que proteger (ver Variaciones protegidas) o un test que la necesite.
   - `DominioException` y `ServicioException` como excepciones de cada capa.
   - La UI solo conoce Servicios y DTOs.
4. **Cada cambio con tests.** Mover comportamiento entre clases es un refactor: los tests
   tienen que estar en verde antes y después. Si una regla pasa del servicio a una
   entidad (Experto), primero se escribe el test en la entidad (RED/GREEN) y recién
   después se borra la lógica del servicio (REFACTOR).

## Conceptos (definiciones del curso)

- **Acoplamiento**: mide la probabilidad de que al modificar una clase el cambio se
  propague a otra. Una clase con bajo acoplamiento depende de pocas clases. Es peor
  cuando se depende de una clase **inestable** (que cambia seguido). Formas: una clase
  que usa a muchas otras; un método que se basa en cómo está implementado otro; la
  herencia (la subclase depende de la implementación del padre).
- **Cohesión**: mide qué tan relacionadas están las responsabilidades de una clase. Una
  clase con baja cohesión hace cosas no afines o un trabajo excesivo: es difícil de
  entender, de reutilizar y la afectan constantemente los cambios. Pregunta guía del
  curso: **¿cuántos motivos para cambiar tiene la clase?** (ejemplo `Employee`: datos,
  cálculo de sueldo, reportes y persistencia = cuatro motivos).
- **El yin yang**: no hay una relación matemática, pero una clase con baja cohesión
  normalmente tiene alto acoplamiento (hace de todo, entonces necesita a todos).
- **Separación de intereses**: cada módulo (clase o paquete) se ocupa de algo bien
  definido (UI, reglas del negocio, persistencia).

## Cómo revisar

Trabajar sobre los archivos que indique el usuario (o el diff de la rama). Si se pide
"todo el proyecto", empezar por `Servicios`, que es donde se concentra el acoplamiento.

### Paso 1. Inventario de cada clase

Para cada clase, armar una fila con datos verificables (citar `archivo:línea`):

| Dato | Cómo se obtiene |
|---|---|
| Dependencias salientes | Campos inyectados por constructor, tipos de otras clases del proyecto que usa en campos, parámetros, variables y `new`. Separar por proyecto (Dominio, Data, DTOs, Servicios, EF/terceros). |
| Dependencias entrantes | Quién la usa: buscar el nombre de la clase con grep en el resto de la solución (sin contar tests). |
| Métodos públicos | Lista. |
| Uso de campos por método | Matriz método × campo: qué campos (o repositorios) usa cada método público, directo o a través de sus privados. |
| Motivos para cambiar | Agrupar los métodos por el tema del que se ocupan (como el ejemplo `Employee`). |

### Paso 2. Señales de acoplamiento alto

- **Muchas dependencias salientes.** Un servicio con 4 o más repositorios o servicios
  inyectados, o una clase que usa clases de 3 o más proyectos. El curso marca como
  problema una clase que depende de otras 3 ("cualquier cambio la afecta").
- **Dependencia de algo inestable o de otra capa**: el dominio que usa EF, `DateTime.Now`
  o DTOs; un servicio que conoce detalles de EF (`Include`, `DbSet`, `SaveChanges`); la UI
  que nombra clases de `Data` o `Dominio` fuera de `Program.cs`; dependencias en el
  sentido equivocado (Data → Servicios, Dominio → Data).
- **Conocimiento de la implementación de otro**: el servicio pide "todos" al repositorio
  y filtra o cuenta en memoria (sabe cómo guarda los datos y además trae todo a memoria,
  que la letra pide evitar); una clase que depende del orden en que otra hace las cosas.
- **Cadenas de navegación** (`a.B.C.Hacer()`): acoplan a la estructura interna de varias
  clases a la vez (Ley de Demeter).
- **La misma regla en varias clases** (por ejemplo, "no tiene límite si el tope es null"
  repetido en un servicio y en la entidad): un cambio de la regla se propaga a todas.
- **Dependencias circulares o de ida y vuelta** entre servicios o entre entidades.
- **Herencia usada para reutilizar código** de una clase que cambia seguido.

### Paso 3. Señales de cohesión baja

- **Más de un motivo para cambiar**: la matriz método × campo se parte en grupos que no
  comparten campos (por ejemplo, la mitad de los métodos usa solo el repositorio A y la
  otra mitad solo el B). Cada grupo es candidato a su propia clase.
- **Nombres que delatan una bolsa de cosas**: `Comunes`, `Utilidades`, `Helper`, `Gestor`,
  `Manager`, `Procesador`, o un nombre de clase que necesita "y" para describirse.
- **Lógica sobre los datos de una sola entidad que vive en el servicio**: el servicio lee
  propiedades de una entidad y decide algo que la entidad podría decidir sola. Viola
  Experto y baja la cohesión de las dos clases.
- **Clases de datos sin comportamiento** cuyas reglas están en otro lado (la entidad solo
  tiene propiedades y el servicio hace todo).
- **Trabajo excesivo**: clases de más de ~150 líneas o con más de ~8 métodos públicos que
  no comparten datos.
- **Métodos que solo delegan** en otro objeto sin agregar nada: no son un error (un
  controlador delega), pero si son la mayoría, la clase puede ser una indirección que no
  protege ninguna variación.

### Paso 4. Proponer la mejora con el patrón GRASP que corresponde

Cada hallazgo se responde con el patrón del curso que lo resuelve:

| Problema | Patrón | Qué hacer |
|---|---|---|
| Una regla usa datos que tiene otra clase | **Experto en información** | Mover la responsabilidad a la clase que tiene la información (en general, las entidades del dominio son las expertas en las reglas del negocio). |
| Quién crea un objeto | **Creador** | Lo crea quien lo contiene, lo registra o tiene los datos para inicializarlo. |
| Un servicio atiende demasiados eventos distintos | **Controlador** | Un servicio por caso de uso (controlador de caso de uso), que **delega** en el dominio y los repositorios en vez de resolver él las reglas. |
| `switch`/`if` según el tipo de algo | **Polimorfismo** | Cada variante se comporta distinto a través de una operación común. |
| Una responsabilidad no le corresponde a ninguna entidad sin bajar su cohesión (persistir, exportar, formatear) | **Fabricación pura** | Clase artificial, cohesiva, que no representa nada del dominio (por ejemplo, un exportador o un repositorio). |
| Dos clases acopladas a peculiaridades de una API o librería | **Indirección** | Un intermediario que se acopla a la API y libera al resto (por ejemplo, la librería de exportación a XLSX detrás de una clase propia). |
| Un punto que se sabe que va a variar | **Variaciones protegidas** | Una interfaz estable alrededor de lo que varía. Solo si la variación es concreta, no "por si acaso". |

Para cada propuesta, indicar:
- **Qué cambia**: clases y métodos que se mueven, se crean o se borran.
- **Antes y después**: dependencias salientes de las clases afectadas (números del paso 1).
- **Costo**: archivos y tests que hay que tocar, código de otros integrantes involucrado.
- **Tests**: qué tests nuevos hacen falta y cuáles se mueven.

**No proponer** abstracciones que hoy no resuelven nada (una interfaz con una sola
implementación y sin variación prevista, una clase nueva que solo pasa llamadas): suben
la cantidad de clases sin bajar el acoplamiento (regla 4 del diseño simple).

## Formato del reporte

```
## Resumen

| Clase | Depende de (salientes) | La usan (entrantes) | Métodos públicos | Motivos para cambiar |
|---|---|---|---|---|

## Hallazgos

### NombreDeLaClase.cs
- **[Acoplamiento | Cohesión] — severidad — `archivo:línea`**
  Qué pasa, con el dato que lo muestra (cantidad de dependencias, grupos de la matriz,
  regla repetida). Por qué importa: qué cambio futuro se propagaría.
  **Propuesta (patrón GRASP):** qué hacer. **Costo:** qué hay que tocar.

## Qué no cambiaría
Lo que parece un problema pero se justifica (con el motivo).
```

Severidad:
- **Alta**: el diseño ya produce un error o lo va a producir con un cambio que la letra
  pide (por ejemplo, una regla duplicada que hay que cambiar en el Obligatorio 2 en un
  solo lugar y quedaría desactualizada en el otro), o una dependencia en el sentido
  equivocado entre capas. Hay que poder describir el caso concreto.
- **Media**: una clase con más de un motivo para cambiar, lógica de una entidad en el
  servicio, o 4 o más dependencias salientes.
- **Baja**: mejoras que reducen una dependencia sin que haya un cambio previsto que la
  necesite.

Cerrar con 1-2 líneas de impresión general. Si la clase está bien, decirlo: el reporte
se usa para decidir qué cambiar, y los hallazgos de relleno le quitan peso a los reales.

## Cómo se registra el uso (para la documentación)

Después de aplicar cambios, anotar en el registro de uso de IA: fecha, clases revisadas,
hallazgos, cuáles se aplicaron (con el commit) y cuáles no (con el motivo). La letra pide
que el uso de la skill sea sistemático y documentado.
