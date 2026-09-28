using System.Globalization;

namespace Calculatrice;

public partial class MainPage : ContentPage
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private string _current = "0";   // nombre en cours de saisie
    private double? _left;           // opérande de gauche
    private string? _operator;       // opérateur en attente
    private bool _newEntry = true;   // le prochain chiffre démarre un nouveau nombre
    private bool _error;

    public MainPage()
    {
        InitializeComponent();
    }

    // ---------- Saisie ----------
    private void OnDigit(object? sender, EventArgs e)
    {
        if (_error) Reset();
        string d = ((Button)sender!).Text;

        if (_newEntry || _current == "0") { _current = d; _newEntry = false; }
        else if (_current.Length < 15) _current += d;

        UpdateDisplay();
    }

    private void OnDecimal(object? sender, EventArgs e)
    {
        if (_error) Reset();

        if (_newEntry) { _current = "0."; _newEntry = false; }
        else if (!_current.Contains('.')) _current += ".";

        UpdateDisplay();
    }

    // ---------- Opérateurs ----------
    private void OnOperator(object? sender, EventArgs e)
    {
        if (_error) return;
        string op = ((Button)sender!).Text;

        if (_left is not null && _operator is not null && !_newEntry)
        {
            if (!Compute()) return;          // enchaînement : 2 + 3 × ...
        }
        else
        {
            _left = Parse(_current);
        }

        _operator = op;
        _newEntry = true;
        OperationLabel.Text = $"{Format(_left!.Value)} {op}";
        UpdateDisplay();
    }

    private void OnEquals(object? sender, EventArgs e)
    {
        if (_error || _left is null || _operator is null) return;

        string expression = $"{Format(_left.Value)} {_operator} {_current} =";
        if (!Compute()) return;

        OperationLabel.Text = expression;
        _left = null;
        _operator = null;
        _newEntry = true;
        UpdateDisplay();
    }

    // ---------- Fonctions ----------
    private void OnClear(object? sender, EventArgs e) => Reset();

    private void OnBackspace(object? sender, EventArgs e)
    {
        if (_error) { Reset(); return; }
        if (_newEntry) return;

        _current = _current.Length > 1 ? _current[..^1] : "0";
        if (_current == "-") _current = "0";
        UpdateDisplay();
    }

    private void OnSign(object? sender, EventArgs e)
    {
        if (_error || _current == "0") return;
        _current = _current.StartsWith('-') ? _current[1..] : "-" + _current;
        UpdateDisplay();
    }

    private void OnPercent(object? sender, EventArgs e)
    {
        if (_error) return;
        _current = Format(Parse(_current) / 100);
        _newEntry = true;
        UpdateDisplay();
    }

    // ---------- Calcul ----------
    private bool Compute()
    {
        double right = Parse(_current);
        double left = _left!.Value;

        double result = _operator switch
        {
            "+" => left + right,
            "−" => left - right,
            "×" => left * right,
            "÷" => right == 0 ? double.NaN : left / right,
            _ => right
        };

        if (double.IsNaN(result))
        {
            ShowError();
            return false;
        }

        _left = result;
        _current = Format(result);
        return true;
    }

    // ---------- Utilitaires ----------
    private static double Parse(string s) => double.Parse(s, Inv);

    // G12 : 12 chiffres significatifs, ce qui évite 0.1 + 0.2 = 0.30000000000000004
    private static string Format(double v) => v.ToString("G12", Inv);

    private void ShowError()
    {
        _error = true;
        _left = null;
        _operator = null;
        _newEntry = true;
        ResultLabel.Text = "Erreur";
        ResultLabel.FontSize = 56;
        OperationLabel.Text = "Division par zéro impossible";
    }

    private void Reset()
    {
        _current = "0";
        _left = null;
        _operator = null;
        _newEntry = true;
        _error = false;
        OperationLabel.Text = "";
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        ResultLabel.Text = _current;
        // Réduit la police pour que les grands nombres ne débordent pas
        ResultLabel.FontSize = _current.Length > 12 ? 30 : _current.Length > 8 ? 42 : 56;
    }
}