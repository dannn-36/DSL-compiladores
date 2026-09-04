using System.Globalization;

namespace CreditDsl.Wpf;

public sealed class EvaluationContext
{
    private readonly Dictionary<string, object?> values = new(StringComparer.OrdinalIgnoreCase);

    public void Set(string name, object? value) => values[name] = value;

    public T Get<T>(string name)
    {
        if (!values.TryGetValue(name, out var value))
            throw new InvalidOperationException($"La variable '{name}' no existe en el contexto.");

        var converted = Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        return converted is null ? default! : (T)converted;
    }

    public object? GetValue(string name) => values.TryGetValue(name, out var value) ? value : null;
}

public abstract class AstNode
{
    public abstract object? Evaluate(EvaluationContext context);
    public abstract string DisplayName { get; }
    public virtual IReadOnlyList<AstNode> Children => Array.Empty<AstNode>();
}

public sealed class VariableNode(string name) : AstNode
{
    public string Name { get; } = name;
    public override object? Evaluate(EvaluationContext context) => context.GetValue(Name);
    public override string DisplayName => $"Variable: {Name}";
}

public sealed class LiteralNode(object? value) : AstNode
{
    public object? Value { get; } = value;
    public override object? Evaluate(EvaluationContext context) => Value;
    public override string DisplayName => $"Literal: {Value}";
}

public abstract class BinaryNode(AstNode left, AstNode right) : AstNode
{
    protected AstNode Left { get; } = left;
    protected AstNode Right { get; } = right;
    public override IReadOnlyList<AstNode> Children => [Left, Right];
}

public sealed class GreaterThanOrEqualNode(AstNode left, AstNode right) : BinaryNode(left, right)
{
    public override object Evaluate(EvaluationContext context) => Convert.ToDecimal(Left.Evaluate(context)) >= Convert.ToDecimal(Right.Evaluate(context));
    public override string DisplayName => "Mayor o igual (>=)";
}

public sealed class AndNode(AstNode left, AstNode right) : BinaryNode(left, right)
{
    public override object Evaluate(EvaluationContext context) => Convert.ToBoolean(Left.Evaluate(context)) && Convert.ToBoolean(Right.Evaluate(context));
    public override string DisplayName => "Y lógico (AND)";
}

public sealed class EqualNode(AstNode left, AstNode right) : BinaryNode(left, right)
{
    public override object Evaluate(EvaluationContext context) => Equals(Left.Evaluate(context), Right.Evaluate(context));
    public override string DisplayName => "Igualdad (==)";
}

public sealed class AssignmentNode(string variableName, AstNode value) : AstNode
{
    public string VariableName { get; } = variableName;
    public AstNode Value { get; } = value;
    public override object? Evaluate(EvaluationContext context)
    {
        var evaluatedValue = Value.Evaluate(context);
        context.Set(VariableName, evaluatedValue);
        return evaluatedValue;
    }
    public override string DisplayName => $"Asignación: {VariableName} = ...";
    public override IReadOnlyList<AstNode> Children => [new VariableNode(VariableName), Value];
}

public sealed class IfStatementNode(AstNode condition, AssignmentNode thenBranch) : AstNode
{
    public AstNode Condition { get; } = condition;
    public AssignmentNode ThenBranch { get; } = thenBranch;
    public bool LastConditionResult { get; private set; }
    public override object? Evaluate(EvaluationContext context)
    {
        LastConditionResult = Convert.ToBoolean(Condition.Evaluate(context));
        return LastConditionResult ? ThenBranch.Evaluate(context) : null;
    }
    public override string DisplayName => "Si ... entonces";
    public override IReadOnlyList<AstNode> Children => [Condition, ThenBranch];
}
