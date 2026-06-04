using UnityEngine;
using System;

public class ReelController : MonoBehaviour
{
    [Header("Visual Cells")]
    [SerializeField] private PoolSystem[] cells;

    [Header("Movement")]
    [SerializeField] private float spinSpeed = 400f;

    [Header("Layout")]
    [SerializeField] private float symbolSpacing = 170f;

    [SerializeField] private float recyclePosition = -255f;

    private RNGService _rngService;

    private bool _isSpinning;
    private bool _isPreparingStop;

    private int _recycleCount;
    private int _stopRecycleCount;

    private SymbolSystem[] _finalSymbols;
    private int _finalSymbolIndex;

    private Action _onReelStopped;

    private float[] validPositions = {340f, 170f, 0f, -170f};


    private bool _waitingFinalAlignment;

    public void Initialize(RNGService rngService)
    {
        _rngService = rngService;
    }

    public void StartSpin(Action onReelStopped)
    {
        _onReelStopped = onReelStopped;

        _isSpinning = true;
        _isPreparingStop = false;

        _recycleCount = 0;
        _finalSymbolIndex = 0;
    }

    public void PrepareStop(
        SymbolSystem[] finalSymbols,
        int extraCycles)
    {
        _finalSymbols = finalSymbols;

        _stopRecycleCount =
            _recycleCount + extraCycles;

        _isPreparingStop = true;
    }

    private void Update()
    {
        if (!_isSpinning)
            return;

        MoveReel();
    }

    private void MoveReel()
    {
        foreach (PoolSystem cell in cells)
        {
            RectTransform rt =
                cell.GetComponent<RectTransform>();

            rt.anchoredPosition +=
                Vector2.down * spinSpeed * Time.deltaTime;
        }

        PoolSystem bottomCell = cells[cells.Length - 1];

        RectTransform bottomRect =
            bottomCell.GetComponent<RectTransform>();

        if (bottomRect.anchoredPosition.y < recyclePosition)
        {
            HandleRecycle(bottomCell);
        }
    }

    private void HandleRecycle(PoolSystem recycledCell)
    {
        _recycleCount++;

        RectTransform recycledRect =
            recycledCell.GetComponent<RectTransform>();

        RectTransform topRect =
            cells[0].GetComponent<RectTransform>();

        recycledRect.anchoredPosition = new Vector2(
            recycledRect.anchoredPosition.x,
            topRect.anchoredPosition.y + symbolSpacing);

        SymbolSystem symbol;

        if (ShouldRevealFinalSymbols())
        {
            symbol = GetNextFinalSymbol();
        }
        else
        {
            symbol = _rngService.GetVisualSymbol();
        }

        recycledCell.ShowSymbol(symbol);

        RotateArray();

        CheckStopCondition();
    }

    private bool ShouldRevealFinalSymbols()
    {
        if (!_isPreparingStop)
            return false;

        return _recycleCount >= _stopRecycleCount;
    }

    private SymbolSystem GetNextFinalSymbol()
    {
        if (_finalSymbolIndex >= _finalSymbols.Length)
            return _finalSymbols[_finalSymbols.Length - 1];

        return _finalSymbols[_finalSymbolIndex++];
    }

    private void CheckStopCondition()
    {
        if (!_isPreparingStop)
            return;

        if (_finalSymbolIndex < _finalSymbols.Length)
            return;

        if (!_waitingFinalAlignment)
        {
            _waitingFinalAlignment = true;
            _stopRecycleCount = _recycleCount + 1;
            return;
        }

        if (_recycleCount >= _stopRecycleCount)
        {
            StopSpin();
        }
    }

    private void StopSpin()
    {
        _isSpinning = false;

        AlignCells();

        _onReelStopped?.Invoke();
    }

    public PoolSystem GetVisibleCell(int row)
    {
        float targetY = row switch
        {
            0 => 170f, // topo
            1 => 0f,   // meio
            2 => -170f,// baixo
            _ => 0f
        };

        foreach (var cell in cells)
        {
            RectTransform rt =
                cell.GetComponent<RectTransform>();

            if (Mathf.Abs(
                rt.anchoredPosition.y - targetY) < 1f)
            {
                return cell;
            }
        }

        return null;
    }

    private void AlignCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            RectTransform rt =
                cells[i].GetComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(
                rt.anchoredPosition.x,
                validPositions[i]);
        }
    }

    private void RotateArray()
    {
        PoolSystem last = cells[cells.Length - 1];

        for (int i = cells.Length - 1; i > 0; i--)
        {
            cells[i] = cells[i - 1];
        }

        cells[0] = last;
    }
}