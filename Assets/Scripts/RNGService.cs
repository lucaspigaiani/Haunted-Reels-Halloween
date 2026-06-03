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

    public SymbolSystem GetVisualSymbol()
    {
        return GetWeightedRandomSymbol();
    }

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

    public void LogSpinResult(SpinResult result)
    {
        Debug.Log("===== SPIN RESULT =====");

        int reels = result.Grid.GetLength(0);
        int rows = result.Grid.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            string line = "";

            for (int reel = 0; reel < reels; reel++)
            {
                line += result.Grid[reel, row].Type + " ";
            }

            Debug.Log(line);
        }
    }

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