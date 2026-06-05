using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PaylineDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ReelController[] reels;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private GameObject linePrefab;

    [Header("Visual")]
    [SerializeField] private float lineWidth = 5f;
    [SerializeField] private float winningLineWidth = 8f;

    [Header("Timing")]
    [SerializeField] private float winningDuration = 1.2f;
    [SerializeField] private float nonWinningDuration = 0.3f;

    private int[,] _paylines;
    private Coroutine displayRoutine;
    private bool isDrawing = false;
    private readonly List<GameObject> activeLines = new List<GameObject>();

    public void SetPaylines(int[,] paylines)
    {
        _paylines = paylines;
    }


    public void DrawPaylinesSequentially(List<PaylineEvaluation> evaluations)
    {
        if (displayRoutine != null)
            StopDrawing();

        isDrawing = true;
        displayRoutine = StartCoroutine(DrawSequentially(evaluations));
    }

    public void StopDrawing()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        ClearAllLines();
        isDrawing = false;
    }

    private IEnumerator DrawSequentially(List<PaylineEvaluation> evaluations)
    {
        // Prepara as células visíveis de cada reel
        List<List<PoolSystem>> visibleCellsPerReel = new List<List<PoolSystem>>();
        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
            PoolSystem[] visibleCells = reels[reelIndex].GetVisibleCells();
            visibleCellsPerReel.Add(new List<PoolSystem>(visibleCells));
        }

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < evaluations.Count; i++)
        {
            if (!isDrawing) yield break;

            PaylineEvaluation eval = evaluations[i];

            DrawSinglePayline(eval.LineIndex, visibleCellsPerReel, eval.IsWinning);

            float displayTime = eval.IsWinning ? winningDuration : nonWinningDuration;
            yield return new WaitForSeconds(displayTime);

            if (isDrawing)
                ClearAllLines();
            else
                yield break;
        }

        isDrawing = false;
        displayRoutine = null;
    }

    private void DrawSinglePayline(int paylineIndex, List<List<PoolSystem>> visibleCellsPerReel, bool isWinning)
    {
        Vector2[] points = new Vector2[5];

        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
           
            int targetRow = _paylines[paylineIndex, reelIndex];
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

        image.color = isWinning ? GetLineColor(lineIndex) : Color.white;
        activeLines.Add(lineObj);
    }

    private Color GetLineColor(int lineIndex)
    {
        Color[] colors =
        {
            Color.yellow, Color.red, Color.green, Color.blue,
            Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f),
            new Color(0.5f, 0f, 0.5f), new Color(1f, 0.8f, 0f),
            new Color(0f, 0.8f, 0.8f)
        };
        return colors[lineIndex % colors.Length];
    }

    private void ClearAllLines()
    {
        foreach (GameObject line in activeLines)
        {
            if (line != null) Destroy(line);
        }
        activeLines.Clear();
    }
}