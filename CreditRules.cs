namespace CreditDsl.Wpf;

public sealed record CreditRule(string Code, string Description, IfStatementNode Ast);

public static class CreditRuleBook
{
    public static IReadOnlyList<CreditRule> CreateRules() =>
    [
        new("R1", "SI edad >= 18 ENTONCES clienteHabilitado = true", If(
            new GreaterThanOrEqualNode(new VariableNode("edad"), new LiteralNode(18)),
            "clienteHabilitado", true)),
        new("R2", "SI ingresos >= 3.000.000 ENTONCES nivelIngresos = \"ALTO\"", If(
            new GreaterThanOrEqualNode(new VariableNode("ingresos"), new LiteralNode(3_000_000m)),
            "nivelIngresos", "ALTO")),
        new("R3", "SI puntaje >= 700 ENTONCES riesgo = \"BAJO\"", If(
            new GreaterThanOrEqualNode(new VariableNode("puntaje"), new LiteralNode(700)),
            "riesgo", "BAJO")),
        new("R4", "SI edad >= 18 Y ingresos >= 3.000.000 Y puntaje >= 700 ENTONCES creditoAprobado = true", If(
            And(new GreaterThanOrEqualNode(new VariableNode("edad"), new LiteralNode(18)),
                And(new GreaterThanOrEqualNode(new VariableNode("ingresos"), new LiteralNode(3_000_000m)),
                    new GreaterThanOrEqualNode(new VariableNode("puntaje"), new LiteralNode(700)))),
            "creditoAprobado", true)),
        new("R5", "SI edad >= 21 Y ingresos >= 5.000.000 ENTONCES segmentoCliente = \"PREMIUM\"", If(
            And(new GreaterThanOrEqualNode(new VariableNode("edad"), new LiteralNode(21)),
                new GreaterThanOrEqualNode(new VariableNode("ingresos"), new LiteralNode(5_000_000m))),
            "segmentoCliente", "PREMIUM")),
        new("R6", "SI puntaje >= 800 ENTONCES tasaPreferencial = true", If(
            new GreaterThanOrEqualNode(new VariableNode("puntaje"), new LiteralNode(800)),
            "tasaPreferencial", true)),
        new("R7", "SI moraActual == false Y antiguedadLaboral >= 12 ENTONCES perfilEstable = true", If(
            new AndNode(new EqualNode(new VariableNode("moraActual"), new LiteralNode(false)),
                new GreaterThanOrEqualNode(new VariableNode("antiguedadLaboral"), new LiteralNode(12))),
            "perfilEstable", true))
    ];

    private static IfStatementNode If(AstNode condition, string name, object value) =>
        new(condition, new AssignmentNode(name, new LiteralNode(value)));

    private static AndNode And(AstNode left, AstNode right) => new(left, right);
}
