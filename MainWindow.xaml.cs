using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CreditDsl.Wpf;

public partial class MainWindow : Window
{
    private readonly IReadOnlyList<CreditRule> rules = CreditRuleBook.CreateRules();
    private readonly List<ResultRow> resultRows = [];

    public MainWindow()
    {
        InitializeComponent();
        RuleSelector.ItemsSource = rules;
        RuleSelector.SelectedIndex = 3;
        EvaluateRequest();
    }

    private void EvaluateButton_Click(object sender, RoutedEventArgs e) => EvaluateRequest();

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        AgeInput.Text = "25";
        IncomeInput.Text = "5000000";
        ScoreInput.Text = "750";
        SeniorityInput.Text = "24";
        DelinquencyInput.Text = "no";
        EvaluateRequest();
    }

    private void EvaluateRequest()
    {
        ValidationText.Text = string.Empty;
        if (!TryReadContext(out var context))
            return;

        resultRows.Clear();
        foreach (var rule in rules)
        {
            var result = rule.Ast.Evaluate(context);
            var assignedValue = context.GetValue(GetAssignedVariable(rule.Ast));
            resultRows.Add(new ResultRow(rule.Code, rule.Description,
                rule.Ast.LastConditionResult ? "CUMPLE" : "NO CUMPLE",
                assignedValue is null ? "-" : FormatValue(assignedValue)));
        }

        ResultsGrid.ItemsSource = null;
        ResultsGrid.ItemsSource = resultRows;
        var approved = Equals(context.GetValue("creditoAprobado"), true);
        SummaryText.Text = approved ? "Solicitud aprobada" : "Solicitud no aprobada";
        SummaryText.Foreground = approved ? new SolidColorBrush(Color.FromRgb(15, 118, 110)) : new SolidColorBrush(Color.FromRgb(185, 28, 28));
        if (RuleSelector.SelectedItem is CreditRule selectedRule)
            BuildTree(selectedRule);
    }

    private bool TryReadContext(out EvaluationContext context)
    {
        context = new EvaluationContext();
        if (!int.TryParse(AgeInput.Text, out var age) || age < 0 ||
            !decimal.TryParse(IncomeInput.Text.Replace(".", "").Replace(",", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out var income) || income < 0 ||
            !int.TryParse(ScoreInput.Text, out var score) || score < 0 ||
            !int.TryParse(SeniorityInput.Text, out var seniority) || seniority < 0)
        {
            ValidationText.Text = "Revise edad, ingresos, puntaje y antigüedad. Use números no negativos.";
            return false;
        }

        var delinquency = DelinquencyInput.Text.Trim().ToLowerInvariant();
        if (delinquency is not ("si" or "sí" or "no"))
        {
            ValidationText.Text = "La mora actual debe ser 'si' o 'no'.";
            return false;
        }

        context.Set("edad", age);
        context.Set("ingresos", income);
        context.Set("puntaje", score);
        context.Set("antiguedadLaboral", seniority);
        context.Set("moraActual", delinquency is "si" or "sí");
        return true;
    }

    private void BuildTree(CreditRule selectedRule)
    {
        AstTree.Items.Clear();
        AstTree.Items.Add(CreateTreeItem(selectedRule.Ast));
        if (AstTree.Items.Count > 0)
            ((TreeViewItem)AstTree.Items[0]).IsExpanded = true;
    }

    private TreeViewItem CreateTreeItem(AstNode node)
    {
        var item = new TreeViewItem { Header = node.DisplayName };
        foreach (var child in node.Children)
            item.Items.Add(CreateTreeItem(child));
        return item;
    }

    private void RuleSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RuleSelector.SelectedItem is CreditRule selectedRule)
        {
            BuildTree(selectedRule);
            SummaryText.Text = $"AST de la regla {selectedRule.Code}";
        }
    }

    private static string GetAssignedVariable(IfStatementNode rule) => rule.ThenBranch.VariableName;
    private static string FormatValue(object value) => value switch
    {
        decimal amount => amount.ToString("N0", CultureInfo.GetCultureInfo("es-CO")),
        bool boolean => boolean ? "true" : "false",
        _ => value.ToString() ?? "-"
    };

    private sealed record ResultRow(string Code, string Description, string Result, string Assignment);
}
