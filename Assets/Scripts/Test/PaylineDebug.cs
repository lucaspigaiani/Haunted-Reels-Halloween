using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PaylineDebug : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ReelController[] reels;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private GameObject linePrefab;

    [Header("Visual")]
    [SerializeField] private float lineWidth = 5f;
    [SerializeField] private float winningLineWidth = 8f;

    [Header("Timing")]
    [SerializeField] private float paylineInterval = 0.15f;
    [SerializeField] private float winningDuration = 1.2f;
    [SerializeField] private float nonWinningDuration = 0.3f;

    private Coroutine displayRoutine;
    private bool isShowingLines = false; // Flag para controlar se está mostrando linhas

    private readonly int[,] paylines =
    {
        {1,1,1,1,1}, // Linha 1 - Meio
        {0,0,0,0,0}, // Linha 2 - Topo
        {2,2,2,2,2}, // Linha 3 - Baixo
        {2,1,0,1,2}, // Linha 4
        {0,1,2,1,0}, // Linha 5
        {0,0,1,2,2}, // Linha 6
        {2,2,1,0,0}, // Linha 7
        {0,1,1,2,2}, // Linha 8
        {2,1,1,0,0}, // Linha 9
        {0,1,1,1,2}  // Linha 10
    };

    private readonly List<GameObject> activeLines = new();
    private Dictionary<int, PaylineInfo> winningLinesCache = new();

    // METODO PUBLICO PARA INTERROMPER A EXIBICAO
    public void ForceStopPaylines()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        ClearAllLines();
        isShowingLines = false;
        Debug.Log("Exibicao de paylines interrompida e limpa");
    }

    public void ShowPaylines(SpinResult spinResult)
    {
        // Interrompe qualquer exibicao anterior imediatamente
        ForceStopPaylines();

        isShowingLines = true;
        displayRoutine = StartCoroutine(ShowPaylinesSequentially(spinResult));
    }

    private IEnumerator ShowPaylinesSequentially(SpinResult spinResult)
    {
        winningLinesCache.Clear();

        List<List<PoolSystem>> visibleCellsPerReel = new();
        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
            PoolSystem[] visibleCells = reels[reelIndex].GetVisibleCells();
            visibleCellsPerReel.Add(new List<PoolSystem>(visibleCells));
        }

        Debug.Log("========== VERIFICANDO PAYLINES ==========");

        for (int paylineIndex = 0; paylineIndex < paylines.GetLength(0); paylineIndex++)
        {
            // Verifica se foi interrompido antes de continuar
            if (!isShowingLines)
            {
                Debug.Log("Exibicao interrompida durante verificacao");
                yield break;
            }

            PaylineInfo info = CheckPaylineWinner(spinResult, paylineIndex);
            winningLinesCache[paylineIndex] = info;

            if (info.isWinning)
            {
                Debug.Log($"[POSITIVO] LINHA {paylineIndex + 1} PREMIADA | {info.count}x {info.symbolName} (incluindo {info.wildCount} Wilds) | Símbolos: {info.symbolsString}");
            }
            else
            {
                Debug.Log($"[NEGATIVO] LINHA {paylineIndex + 1} - Nao premiada | Melhor: {info.bestCount}x {info.bestSymbol} + {info.wildCount} Wilds = {info.count}x | Símbolos: {info.symbolsString}");
            }
        }

        Debug.Log("==========================================");
        yield return new WaitForSeconds(0.2f);

        for (int paylineIndex = 0; paylineIndex < paylines.GetLength(0); paylineIndex++)
        {
            // Verifica se foi interrompido antes de mostrar cada linha
            if (!isShowingLines)
            {
                Debug.Log("Exibicao interrompida durante desenho das linhas");
                ClearAllLines();
                yield break;
            }

            PaylineInfo info = winningLinesCache[paylineIndex];
            DrawSinglePayline(paylineIndex, visibleCellsPerReel, info.isWinning);

            float displayTime = info.isWinning ? winningDuration : nonWinningDuration;
            yield return new WaitForSeconds(displayTime);

            // Remove apenas a linha atual se ainda estiver ativo
            if (isShowingLines)
            {
                ClearAllLines();
            }
            else
            {
                yield break;
            }
        }

        isShowingLines = false;
        displayRoutine = null;
    }

    private PaylineInfo CheckPaylineWinner(SpinResult spinResult, int paylineIndex)
    {
        PaylineInfo info = new PaylineInfo();

        // Pega os símbolos da payline
        List<string> symbols = new List<string>();
        for (int reel = 0; reel < 5; reel++)
        {
            int row = paylines[paylineIndex, reel];
            SymbolSystem symbol = spinResult.Grid[reel, row];
            symbols.Add(symbol.Type.ToString());
        }

        info.symbolsString = string.Join(" ", symbols);

        // Conta quantos Wilds tem na linha
        int wildCount = symbols.Count(s => s == "Wild");
        info.wildCount = wildCount;

        // Conta a frequência de cada símbolo (excluindo Wilds)
        Dictionary<string, int> symbolCounts = new Dictionary<string, int>();

        foreach (string symbol in symbols)
        {
            if (symbol == "Wild") continue;

            if (!symbolCounts.ContainsKey(symbol))
                symbolCounts[symbol] = 0;

            symbolCounts[symbol]++;
        }

        // Se só tem Wilds na linha
        if (symbolCounts.Count == 0)
        {
            info.isWinning = wildCount >= 3;
            info.count = wildCount;
            info.symbolName = "Wild";
            info.bestCount = wildCount;
            info.bestSymbol = "Wild";
            return info;
        }

        // Encontra o símbolo com maior contagem
        string bestSymbol = "";
        int bestCount = 0;

        foreach (var kvp in symbolCounts)
        {
            if (kvp.Value > bestCount)
            {
                bestCount = kvp.Value;
                bestSymbol = kvp.Key;
            }
        }

        // Soma os Wilds ao melhor símbolo
        int totalCount = bestCount + wildCount;

        info.bestCount = bestCount;
        info.bestSymbol = bestSymbol;
        info.count = totalCount;
        info.symbolName = bestSymbol;
        info.isWinning = totalCount >= 3;

        return info;
    }

    private void DrawSinglePayline(int paylineIndex, List<List<PoolSystem>> visibleCellsPerReel, bool isWinning)
    {
        Vector2[] points = new Vector2[5];

        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
            int targetRow = paylines[paylineIndex, reelIndex];
            PoolSystem selectedCell = visibleCellsPerReel[reelIndex][targetRow];
            points[reelIndex] = selectedCell.transform.position;
        }

        for (int i = 0; i < points.Length - 1; i++)
        {
            CreateLineSegment(points[i], points[i + 1], paylineIndex, isWinning);
        }
    }

    private void CreateLineSegment(Vector2 start, Vector2 end, int lineIndex, bool isWinning)
    {
        GameObject lineObj = Instantiate(linePrefab, canvasRect);
        lineObj.SetActive(true);

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        Image image = lineObj.GetComponent<Image>();

        Vector2 direction = end - start;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector2 center = (start + end) * 0.5f;

        rect.position = center;
        float currentLineWidth = isWinning ? winningLineWidth : lineWidth;
        rect.sizeDelta = new Vector2(distance, currentLineWidth);
        rect.rotation = Quaternion.Euler(0, 0, angle);

        if (isWinning)
        {
            image.color = GetLineColor(lineIndex);
        }
        else
        {
            image.color = Color.white;
        }

        activeLines.Add(lineObj);
    }

    private Color GetLineColor(int lineIndex)
    {
        Color[] colors =
        {
            Color.yellow,
            Color.red,
            Color.green,
            Color.blue,
            Color.cyan,
            Color.magenta,
            new Color(1f, 0.5f, 0f),
            new Color(0.5f, 0f, 0.5f),
            new Color(1f, 0.8f, 0f),
            new Color(0f, 0.8f, 0.8f)
        };

        return colors[lineIndex % colors.Length];
    }

    private void ClearAllLines()
    {
        foreach (GameObject line in activeLines)
        {
            if (line != null)
                Destroy(line);
        }

        activeLines.Clear();
    }

    // Metodo alternativo para limpar apenas uma linha especifica
    private void ClearSingleLine(GameObject line)
    {
        if (line != null && activeLines.Contains(line))
        {
            activeLines.Remove(line);
            Destroy(line);
        }
    }

    private class PaylineInfo
    {
        public bool isWinning;
        public int count;
        public int bestCount;
        public int wildCount;
        public string symbolName;
        public string bestSymbol;
        public string symbolsString;
    }
}