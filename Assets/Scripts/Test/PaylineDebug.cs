using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PaylineDebug : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ReelController[] reels;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private GameObject linePrefab;

    [Header("Visual")]
    [SerializeField] private float lineWidth = 5f;

    [Header("Timing")]
    [SerializeField] private float paylineInterval = 0.1f;
    [SerializeField] private float displayDuration = 0.4f;

    private Coroutine displayRoutine;

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

    private readonly List<GameObject> activeLines = new();

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = StartCoroutine(ShowPaylinesSequentially());
    }

    private IEnumerator ShowPaylinesSequentially()
    {
        ClearLines();

        List<List<PoolSystem>> visibleCellsPerReel = new();

        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
            PoolSystem[] visibleCells = reels[reelIndex].GetVisibleCells();

            visibleCellsPerReel.Add(new List<PoolSystem>(visibleCells));
        }

        for (int paylineIndex = 0; paylineIndex < paylines.GetLength(0); paylineIndex++)
        {
            DrawSinglePayline(paylineIndex, visibleCellsPerReel);

            yield return new WaitForSeconds(paylineInterval);
        }

        yield return new WaitForSeconds(displayDuration);

        ClearLines();

        displayRoutine = null;
    }

    private void DrawSinglePayline(int paylineIndex, List<List<PoolSystem>> visibleCellsPerReel)
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
            CreateLineSegment(points[i], points[i + 1], paylineIndex);
        }
    }

    private void CreateLineSegment(Vector2 start, Vector2 end, int lineIndex)
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

        rect.sizeDelta = new Vector2(distance,lineWidth);

        rect.rotation = Quaternion.Euler(0, 0, angle);

        image.color = GetLineColor(lineIndex);

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
            Color.white,
            new Color(1f, 0.5f, 0f),
            Color.gray,
            Color.black
        };

        return colors[lineIndex % colors.Length];
    }

    private void ClearLines()
    {
        foreach (GameObject line in activeLines)
        {
            if (line != null)
                Destroy(line);
        }

        activeLines.Clear();
    }
}