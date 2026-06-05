using System.Collections.Generic;
using UnityEngine;

public class RNGService : MonoBehaviour
{
    [SerializeField]
    private List<SymbolSystem> symbols = new();

    [SerializeField]
    private int reels = 5;

    [SerializeField]
    private int rows = 3;

    /// <summary>
    /// Retorna um símbolo aleatório baseado no peso (Weight) de cada símbolo
    /// Chamado pelo ReelController durante o spin para obter símbolos visuais
    /// </summary>
    public SymbolSystem GetVisualSymbol()
    {
        return GetWeightedRandomSymbol();
    }

    /// <summary>
    /// Gera um resultado completo de spin para todas as posições do grid
    /// Chamado pelo SpinController para determinar o resultado final da rodada
    /// </summary>
    public SpinResult GenerateSpinResult()
    {
        SpinResult result = new SpinResult(reels, rows);

        for (int reel = 0; reel < reels; reel++)
        {
            for (int row = 0; row < rows; row++)
            {
                result.Grid[reel, row] = GetWeightedRandomSymbol();
            }
        }

        return result;
    }

    /// <summary>
    /// Seleciona um símbolo aleatório baseado no peso configurado
    /// Quanto maior o Weight, maior a chance de ser selecionado
    /// </summary>
    private SymbolSystem GetWeightedRandomSymbol()
    {
        int totalWeight = 0;

        foreach (var symbol in symbols)
        {
            totalWeight += symbol.Weight;
        }

        int randomValue = Random.Range(0, totalWeight);

        int currentWeight = 0;

        foreach (var symbol in symbols)
        {
            currentWeight += symbol.Weight;

            if (randomValue < currentWeight)
            {
                return symbol;
            }
        }

        return symbols[0];
    }
}