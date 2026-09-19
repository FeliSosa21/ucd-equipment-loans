---
name: csharp-code-quality-review
description: Revisa clases C# contra los lineamientos de Clean Code (capítulos 1-10 y 12) y las convenciones de codificación de C# de Microsoft. Usar cuando el usuario pida revisar calidad de código, chequear Clean Code, o antes de commitear una clase nueva o modificada del proyecto UCD (Obligatorio 1, DA1).
---

# Revisión de calidad de código C# (Clean Code + convenciones .NET)

## Restricción importante — leer primero

Este proyecto tiene 3 funcionalidades de desarrollo manual sin asistencia de IA:
**Registro en el sistema, Consultas al catálogo, y Préstamos**.

Antes de revisar cualquier archivo, preguntá al usuario si el código pertenece a
alguna de esas 3 funcionalidades. Si la respuesta es sí, NO reescribas ni sugieras
código concreto — solo podés señalar en términos generales qué principio de Clean
Code podría estar en juego (ej. "este método parece tener más de una responsabilidad"),
sin mostrar cómo arreglarlo. Para el resto del proyecto (dominio de otras funcionalidades,
capa de datos, UI, etc.) no hay restricción y podés sugerir cambios concretos.

## Cómo revisar

Leé el/los archivo(s) que el usuario indique (o el diff de los últimos cambios si
pide "revisar lo último que hice"). Chequeá cada punto de la lista de abajo. Para
cada violación encontrada, indicá: archivo, línea aproximada, qué principio viola,
y por qué importa (no solo "está mal", explicar el motivo).

No hace falta que reportes cada punto que SÍ se cumple — enfocate en lo que falta
o está mal. Si el código está limpio, decilo brevemente y no inventes objeciones
menores solo para tener algo que decir.

### 1. Nombres (Clean Code cap. 2)
- Nombres que revelan intención (`cantSolicitada`, no `n` o `cant`)
- Sin abreviaturas crípticas, sin nombres de una sola letra salvo índices de loop cortos
- Clases: sustantivos en PascalCase (`Prestamo`, no `prestamo` ni `Prestamos_Class`)
- Métodos: verbos en PascalCase (`ModificarNombre`, no `modificar_nombre` ni `Nombre`)
- Sin espacios, sin tildes, sin ñ (convención propia del proyecto, ver contexto UML)

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
  (revisar contra el diagrama de dominio — ¿esa propiedad tiene un método propio para modificarse,
  tipo `ModificarNombre`, o se está exponiendo un `set` público que rompe esa regla?)
- Evitar exponer estructura interna innecesariamente

### 6. Manejo de errores (Clean Code cap. 7)
- Excepciones en vez de códigos de error o valores mágicos de retorno
- Mensajes de excepción descriptivos (no genéricos tipo "Error")
- No tragarse excepciones con catch vacíos

### 7. Límites y clases (Clean Code cap. 10)
- Clases con responsabilidad única (¿esta clase hace más de una cosa? ¿debería dividirse?)
- Alta cohesión (los métodos de la clase usan la mayoría de sus atributos)

### 8. Convenciones de C# (Microsoft, linkeadas en la letra)
- PascalCase: clases, métodos, propiedades, eventos, namespaces
- camelCase: variables locales, parámetros
- `_camelCase` con guion bajo: campos privados (si el proyecto los usa; si no aparecen
  campos privados directos, ver si todo pasa por propiedades autoimplementadas, que es
  el estilo que ya viene usando el proyecto)
- `namespace` file-scoped (`namespace Dominio;`) — es el estilo que ya usa este proyecto,
  mantenerlo consistente en archivos nuevos
- Usar `var` cuando el tipo es obvio por el contexto (lado derecho de la asignación)

## Formato del reporte

Organizá los hallazgos por archivo. Para cada uno:

```
### NombreDelArchivo.cs

- **Línea X** — [categoría, ej: Nombres / Funciones / Manejo de errores]
  Qué está mal y por qué importa.
```

Al final, un resumen de 1-2 líneas con la impresión general (no una lista repetida).
