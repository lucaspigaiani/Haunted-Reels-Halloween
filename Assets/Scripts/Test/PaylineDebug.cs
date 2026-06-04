using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class PaylineDebug : MonoBehaviour
{
    [SerializeField] private ReelController[] reels;
    [SerializeField] private RectTransform canvasRect; // Referência ao Canvas
    [SerializeField] private GameObject linePrefab; // Prefab com Image (ou cria dinamicamente)
    [SerializeField] private float lineWidth = 10f;
    [SerializeField] private Color lineColor = Color.yellow;
    [SerializeField] private float displayDuration = 2f;

    private readonly int[,] paylines =
    {
        {1,1,1,1,1},
        {0,0,0,0,0},
        {2,2,2,2,2},
        {2,1,0,1,2},
        {0,1,2,1,0},
        {0,0,1,2,2},
        {2,2,1,0,0},
        {0,1,1,2,2},
        {2,1,1,0,0},
        {0,1,1,1,2}
    };

    private List<GameObject> activeLines = new List<GameObject>();
    private List<List<PoolSystem>> visibleCellsPerReel = new List<List<PoolSystem>>();

    void Start()
    {
        // Cria o prefab de linha dinamicamente se não existir
        if (linePrefab == null)
        {
            CreateLinePrefab();
        }
    }

    void CreateLinePrefab()
    {
        linePrefab = new GameObject("Line", typeof(Image));
        linePrefab.transform.SetParent(canvasRect);
        var image = linePrefab.GetComponent<Image>();
        image.color = lineColor;
        linePrefab.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Chama UMA vez e desenha TODAS as 10 linhas
            DrawAllPaylines();
        }
    }

    void DrawAllPaylines()
    {
        // Limpa linhas anteriores
        ClearLines();

        // Pega células visíveis uma única vez
        visibleCellsPerReel.Clear();
        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
            PoolSystem[] visibleCells = reels[reelIndex].GetVisibleCells();
            visibleCellsPerReel.Add(new List<PoolSystem>(visibleCells));
        }

        // Desenha todas as 10 paylines
        for (int i = 0; i < paylines.GetLength(0); i++)
        {
            DrawSinglePayline(i);
        }

        // Agenda para desligar depois do tempo
        StartCoroutine(HideLinesAfterDelay(displayDuration));
    }

    void DrawSinglePayline(int paylineIndex)
    {
        List<Vector2> screenPoints = new List<Vector2>();

        // Coleta as células da payline e converte para posições da tela
        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
            int targetRow = paylines[paylineIndex, reelIndex];
            PoolSystem selectedCell = visibleCellsPerReel[reelIndex][targetRow];

            if (selectedCell != null)
            {
                // Converte posição do mundo para tela
                Vector2 screenPos = Camera.main.WorldToScreenPoint(selectedCell.transform.position);
                screenPoints.Add(screenPos);
            }
        }

        // Cria a linha entre os pontos
        CreateUILine(screenPoints, paylineIndex);
    }

    void CreateUILine(List<Vector2> points, int lineIndex)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            CreateLineSegment(points[i], points[i + 1], lineIndex);
        }
    }

    void CreateLineSegment(Vector2 start, Vector2 end, int lineIndex)
    {
        GameObject lineObj = Instantiate(linePrefab, canvasRect);
        lineObj.SetActive(true);

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        Image image = lineObj.GetComponent<Image>();

        // Calcula distância e ângulo
        Vector2 direction = end - start;
        float distance = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Posiciona e rotaciona a linha
        rect.anchoredPosition = start;
        rect.sizeDelta = new Vector2(distance, lineWidth);
        rect.rotation = Quaternion.Euler(0, 0, angle);

        // Define cor baseada no índice da linha
        Color lineColorVariation = GetLineColor(lineIndex);
        image.color = lineColorVariation;

        activeLines.Add(lineObj);
    }

    Color GetLineColor(int lineIndex)
    {
        // Cores diferentes para cada linha
        Color[] colors = {
            Color.yellow, Color.red, Color.green, Color.blue, Color.cyan,
            Color.magenta, Color.white, new Color(1, 0.5f, 0), Color.gray, Color.black
        };
        return colors[lineIndex % colors.Length];
    }

    void ClearLines()
    {
        foreach (var line in activeLines)
        {
            Destroy(line);
        }
        activeLines.Clear();
    }

    IEnumerator HideLinesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearLines();
    }
}