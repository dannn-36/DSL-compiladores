# Credit DSL WPF

Sistema académico para representar y evaluar reglas de aprobación de crédito mediante un AST en C# y WPF.

## Requisitos

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 con la carga **Desarrollo de escritorio .NET** o VS Code con C# Dev Kit

## Ejecución

```powershell
dotnet restore
dotnet run
```

La interfaz permite ingresar edad, ingresos, puntaje, antigüedad laboral y estado de mora. El botón **Evaluar solicitud** ejecuta las cuatro reglas solicitadas y tres reglas adicionales, mostrando el resultado y la asignación producida. El panel derecho visualiza el AST de la regla principal de aprobación.

## Reglas adicionales

- R5: edad >= 21 e ingresos >= 5.000.000 -> segmentoCliente = "PREMIUM".
- R6: puntaje >= 800 -> tasaPreferencial = true.
- R7: moraActual == false y antiguedadLaboral >= 12 -> perfilEstable = true.

## Equivalencias conceptuales

- `AstNode`: clase abstracta base de todos los nodos.
- `VariableNode`: lee una variable del `EvaluationContext`.
- `GreaterThanOrEqualNode`: compara dos valores con `>=`.
- `IfStatementNode`: evalúa una condición y ejecuta una asignación cuando es verdadera.
- `EvaluationContext`: diccionario de variables de entrada y salida.

La solución corresponde a un DSL interno porque el lenguaje de reglas se expresa construyendo objetos C# que forman el AST; no existe un parser independiente para archivos escritos por usuarios finales.
