using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class PaylineSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ReelAnimator reelAnimator;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip evaluateMusic;
    [SerializeField] private AudioClip winMusic;
    [SerializeField] private AudioClip payoutSfx;

    private readonly int[,] paylines =
    {
        {1,1,1,1,1}, // Middle
        {0,0,0,0,0}, // Top
        {2,2,2,2,2}, // Bottom
        {2,1,0,1,2}, // V invertido
        {0,1,2,1,0}, // V normal
        {0,0,1,2,2}, // Diagonal descendente
        {2,2,1,0,0}, // Diagonal ascendente
        {0,1,1,2,2}, // Escada descendente
        {2,1,1,0,0}, // Escada ascendente
        {0,1,1,1,2}  // Onda suave
    };

    public PaylineResult Evaluate(SpinResult spinResult, int totalBet)
    {
        PlayEvaluateMusic();

        PaylineResult result = new PaylineResult();

        // Calcula a aposta por linha (aposta total ÷ 10)
        float lineBet = totalBet / 10f;

        // StringBuilder para acumular todo o debug
        StringBuilder debugLog = new StringBuilder();
        debugLog.AppendLine("========== AVALIACAO DE PAYLINES ==========");
        debugLog.AppendLine($"Aposta total: {totalBet} | Aposta por linha: {lineBet:F2}");
        debugLog.AppendLine("----------------------------------------");

        for (int lineIndex = 0; lineIndex < paylines.GetLength(0); lineIndex++)
        {
            EvaluateLine(spinResult, lineIndex, lineBet, result, debugLog);
        }

        debugLog.AppendLine("----------------------------------------");
        debugLog.AppendLine($"TOTAL GANHO: {result.TotalWin:F2} creditos");
        debugLog.AppendLine("=========================================");

        // Printa tudo de uma vez
        Debug.Log(debugLog.ToString());

        if (result.TotalWin > 0)
        {
            PlayWinEffects(result);
        }

        return result;
    }

    private void EvaluateLine(SpinResult spinResult, int lineIndex, float lineBet, PaylineResult result, StringBuilder debugLog)
    {
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

        string symbolsString = string.Join(" ", symbolNames);

        // Conta quantos Wilds tem na linha
        int wildCount = symbols.Count(s => s.Type == SymbolSystem.SymbolType.Wild);

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
            if (wildCount >= 3)
            {
                float payout = CalculatePayoutForWilds(wildCount, lineBet);
                result.TotalWin += payout;
                result.WinningLines.Add(lineIndex);

                debugLog.AppendLine($"[POSITIVO] LINHA {lineIndex + 1,2} | {wildCount}x WILD | Simbolos: {symbolsString} | Ganho: {payout:F2}");
            }
            else
            {
                debugLog.AppendLine($"[NEGATIVO] LINHA {lineIndex + 1,2} | {wildCount}x WILD (minimo 3) | Simbolos: {symbolsString} | Ganho: 0,00");
            }
            return;
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

        // Soma os Wilds ao melhor símbolo
        int totalCount = bestCount + wildCount;

        // Verifica se atingiu pelo menos 3 símbolos
        if (totalCount >= 3)
        {
            // Encontra o símbolo correspondente para pegar os multiplicadores
            SymbolSystem winningSymbol = symbols.Find(s => s.Type == bestSymbolType);

            if (winningSymbol != null)
            {
                float payout = CalculatePayout(winningSymbol, totalCount, lineBet);
                float multiplier = GetMultiplier(winningSymbol, totalCount);

                if (payout > 0)
                {
                    result.TotalWin += payout;
                    result.WinningLines.Add(lineIndex);

                    debugLog.AppendLine($"[POSITIVO] LINHA {lineIndex + 1,2} | {totalCount}x {bestSymbolType} | " +
                                       $"{bestCount} simb + {wildCount} wild = {totalCount} | " +
                                       $"{multiplier}x{lineBet:F2} = {payout:F2} | Simbolos: {symbolsString}");
                }
            }
        }
        else
        {
            debugLog.AppendLine($"[NEGATIVO] LINHA {lineIndex + 1,2} | Melhor: {bestCount}x {bestSymbolType} + {wildCount}x wild = {totalCount}x (minimo 3) | Simbolos: {symbolsString}");
        }
    }

    private float CalculatePayout(SymbolSystem symbol, int matches, float lineBet)
    {
        float multiplier = 0;

        switch (matches)
        {
            case 3:
                multiplier = symbol.Multiplier3;
                break;
            case 4:
                multiplier = symbol.Multiplier4;
                break;
            case 5:
                multiplier = symbol.Multiplier5;
                break;
            default:
                return 0;
        }

        // Calcula o ganho: multiplicador × aposta por linha
        float payout = multiplier * lineBet;

        // Arredonda para 2 casas decimais
        return Mathf.Round(payout * 100f) / 100f;
    }

    private float CalculatePayoutForWilds(int matches, float lineBet)
    {
        float multiplier = 0;
        switch (matches)
        {
            case 3:
                multiplier = 5f; // Mesmo que H1
                break;
            case 4:
                multiplier = 25f; // Mesmo que H1
                break;
            case 5:
                multiplier = 150f; // Mesmo que H1
                break;
            default:
                return 0;
        }

        float payout = multiplier * lineBet;
        return Mathf.Round(payout * 100f) / 100f;
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

    private void PlayEvaluateMusic()
    {
        if (musicSource == null || evaluateMusic == null) return;

        musicSource.clip = evaluateMusic;
        musicSource.Play();
    }

    private void PlayWinEffects(PaylineResult result)
    {
        if (musicSource != null && winMusic != null)
        {
            musicSource.clip = winMusic;
            musicSource.Play();
        }

        if (payoutSfx != null)
        {
            AudioSource.PlayClipAtPoint(payoutSfx, Vector3.zero);
        }

        if (reelAnimator != null)
        {
            reelAnimator.PlayIdle();
        }
    }
}