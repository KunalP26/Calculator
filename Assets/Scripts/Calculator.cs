using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A simple calculator to evaluate arithmetic expressions entered via UI buttons.
/// </summary>
public class Calculator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText; // Text display for the calculator
    private string _currentExpression = "";

    /// <summary>
    /// Called when a button is pressed on the calculator.
    /// </summary>
    /// <param name="input">The input string from the button (e.g., numbers, operators, or commands).</param>
    public void OnButtonPress(string input)
    {
        switch (input)
        {
            case "=":
                EvaluateCurrentExpression();
                break;
            case "C":
                ClearExpression();
                break;
            default:
                AppendToExpression(input);
                break;
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Evaluates the current arithmetic expression and updates the result or displays an error.
    /// </summary>
    private void EvaluateCurrentExpression()
    {
        try
        {
            double result = EvaluateExpression(_currentExpression);
            _currentExpression = result.ToString();
        }
        catch (DivideByZeroException ex)
        {
            _currentExpression = "Error: Divide by Zero";
            Debug.LogError(ex.Message);
        }
        catch (Exception ex)
        {
            _currentExpression = "Error";
            Debug.LogError($"Error evaluating expression: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears the current expression.
    /// </summary>
    private void ClearExpression()
    {
        _currentExpression = "";
    }

    /// <summary>
    /// Appends input to the current expression, ensuring the input is valid.
    /// </summary>
    /// <param name="input">The input to append.</param>
    private void AppendToExpression(string input)
    {
        if ("+-*/".Contains(input) && 
            (_currentExpression.Length == 0 || "+-*/".Contains(_currentExpression[^1].ToString())))
        {
            // Avoid starting with an operator or appending consecutive operators
            return;
        }

        _currentExpression += input;
    }

    /// <summary>
    /// Updates the calculator's display text.
    /// </summary>
    private void UpdateDisplay()
    {
        displayText.text = string.IsNullOrEmpty(_currentExpression) ? "0" : _currentExpression;
    }

    /// <summary>
    /// Evaluates a mathematical expression represented as a string.
    /// </summary>
    /// <param name="expression">The arithmetic expression to evaluate.</param>
    /// <returns>The result of the evaluation as a double.</returns>
    private double EvaluateExpression(string expression)
    {
        Stack<double> values = new Stack<double>();
        Stack<char> operators = new Stack<char>();
        int i = 0;

        while (i < expression.Length)
        {
            char c = expression[i];

            if (char.IsDigit(c) || c == '.')
            {
                string number = ParseNumber(expression, ref i);
                values.Push(double.Parse(number));
            }
            else if ("+-*/".Contains(c))
            {
                ProcessOperator(c, values, operators);
                i++;
            }
            else
            {
                i++;
            }
        }

        ApplyRemainingOperations(values, operators);
        return values.Pop();
    }

    /// <summary>
    /// Parses a number from the expression.
    /// </summary>
    private string ParseNumber(string expression, ref int index)
    {
        string number = "";

        while (index < expression.Length && (char.IsDigit(expression[index]) || expression[index] == '.'))
        {
            number += expression[index];
            index++;
        }

        return number;
    }

    /// <summary>
    /// Processes an operator by applying higher-precedence operations first.
    /// </summary>
    private void ProcessOperator(char currentOperator, Stack<double> values, Stack<char> operators)
    {
        while (operators.Count > 0 && Precedence(operators.Peek()) >= Precedence(currentOperator))
        {
            ApplyOperation(values, operators.Pop());
        }

        operators.Push(currentOperator);
    }

    /// <summary>
    /// Applies all remaining operations in the operator stack.
    /// </summary>
    private void ApplyRemainingOperations(Stack<double> values, Stack<char> operators)
    {
        while (operators.Count > 0)
        {
            ApplyOperation(values, operators.Pop());
        }
    }

    /// <summary>
    /// Returns the precedence level of an operator.
    /// </summary>
    private int Precedence(char op) => (op == '+' || op == '-') ? 1 : 2;

    /// <summary>
    /// Applies a single operation on the value stack.
    /// </summary>
    private void ApplyOperation(Stack<double> values, char op)
    {
        double b = values.Pop();
        double a = values.Pop();

        values.Push(op switch
        {
            '+' => a + b,
            '-' => a - b,
            '*' => a * b,
            '/' => b == 0 ? throw new DivideByZeroException("Cannot divide by zero.") : a / b,
            _ => throw new InvalidOperationException($"Invalid operator: {op}")
        });
    }
}
