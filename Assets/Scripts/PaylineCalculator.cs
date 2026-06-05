using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PaylineCalculator
{
    private readonly int[,] paylines;

    public PaylineCalculator(int[,] paylines)
    {
        this.paylines = paylines;
    }

    public List<PaylineEvaluation> EvaluateAllLines(SpinResult spinResult, float lineBet)
    {
        List<PaylineEvaluation> evaluations = new List<PaylineEvaluation>();

        for (int lineIndex = 0; lineIndex < paylines.GetLength(0); lineIndex++)
        {
            PaylineEvaluation evaluation = EvaluateLine(spinResult, lineIndex, lineBet);
            evaluations.Add(evaluation);
        }

        return evaluations;
    }

    public PaylineEvaluation EvaluateLine(SpinResult spinResult, int lineIndex, float lineBet)
    {
        PaylineEvaluation evaluation = new PaylineEvaluation();
        evaluation.LineIndex = lineIndex;
        evaluation.LineBet = lineBet;

        // Pega os símbolos da payline
        List<SymbolSystem> symbols = new List<SymbolSystem>();
        List<string> symbolNames = new List<string>();

        for (int reel = 0; reel < 5; reel++)
        {
            int row = paylines[lineIndex, reel];
            SymbolSystem symbol = spinResult.Grid[reel, row];
            symbols.Add(symbol);
            symbolNames.Add(symbol.Type.ToString());
        }

        evaluation.SymbolsString = string.Join(" ", symbolNames);
        evaluation.Symbols = symbols;

        // Conta quantos Wilds tem na linha
        int wildCount = symbols.Count(s => s.Type == SymbolSystem.SymbolType.Wild);
        evaluation.WildCount = wildCount;

        // Conta a frequência de cada símbolo (excluindo Wilds)
        Dictionary<SymbolSystem.SymbolType, int> symbolCounts = new Dictionary<SymbolSystem.SymbolType, int>();

        foreach (SymbolSystem symbol in symbols)
        {
            if (symbol.Type == SymbolSystem.SymbolType.Wild) continue;

            if (!symbolCounts.ContainsKey(symbol.Type))
                symbolCounts[symbol.Type] = 0;

            symbolCounts[symbol.Type]++;
        }

        // Se só tem Wilds na linha
        if (symbolCounts.Count == 0)
        {
            evaluation.BestCount = wildCount;
            evaluation.BestSymbolType = SymbolSystem.SymbolType.Wild;
            evaluation.TotalCount = wildCount;
            evaluation.SymbolName = "Wild";
            evaluation.IsWinning = wildCount >= 3;

            if (evaluation.IsWinning)
            {
                evaluation.Multiplier = GetWildMultiplier(wildCount);
                evaluation.Payout = evaluation.Multiplier * lineBet;
                evaluation.Payout = Mathf.Round(evaluation.Payout * 100f) / 100f;
            }
            return evaluation;
        }

        // Encontra o símbolo com maior contagem
        SymbolSystem.SymbolType bestSymbolType = SymbolSystem.SymbolType.Wild;
        int bestCount = 0;

        foreach (var kvp in symbolCounts)
        {
            if (kvp.Value > bestCount)
            {
                bestCount = kvp.Value;
                bestSymbolType = kvp.Key;
            }
        }

        evaluation.BestCount = bestCount;
        evaluation.BestSymbolType = bestSymbolType;
        evaluation.TotalCount = bestCount + wildCount;
        evaluation.SymbolName = bestSymbolType.ToString();
        evaluation.IsWinning = evaluation.TotalCount >= 3;

        if (evaluation.IsWinning)
        {
            SymbolSystem winningSymbol = symbols.Find(s => s.Type == bestSymbolType);
            if (winningSymbol != null)
            {
                evaluation.Multiplier = GetMultiplier(winningSymbol, evaluation.TotalCount);
                evaluation.Payout = evaluation.Multiplier * lineBet;
                evaluation.Payout = Mathf.Round(evaluation.Payout * 100f) / 100f;
            }
        }

        return evaluation;
    }

    private float GetMultiplier(SymbolSystem symbol, int matches)
    {
        return matches switch
        {
            3 => symbol.Multiplier3,
            4 => symbol.Multiplier4,
            5 => symbol.Multiplier5,
            _ => 0
        };
    }

    private float GetWildMultiplier(int matches)
    {
        return matches switch
        {
            3 => 5f,
            4 => 25f,
            5 => 150f,
            _ => 0
        };
    }
}

public class PaylineEvaluation
{
    public int LineIndex;
    public float LineBet;
    public bool IsWinning;
    public int TotalCount;
    public int BestCount;
    public int WildCount;
    public string SymbolName;
    public SymbolSystem.SymbolType BestSymbolType;
    public float Multiplier;
    public float Payout;
    public string SymbolsString;
    public List<SymbolSystem> Symbols;
}