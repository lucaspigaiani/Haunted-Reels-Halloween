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
    private Action _onReelStopped;
    private float[] validPositions = { 340f, 170f, 0f, -170f };

    /// <summary>
    /// Inicializa o ReelController com o serviço de RNG
    /// Chamado pelo SpinController ao iniciar
    /// </summary>
    public void Initialize(RNGService rngService)
    {
        _rngService = rngService;
    }

    /// <summary>
    /// Inicia o movimento de rotação do reel
    /// Chamado pelo SpinController quando o spin começa
    /// </summary>
    public void StartSpin(Action onReelStopped)
    {
        _onReelStopped = onReelStopped;
        _isSpinning = true;
        _isPreparingStop = false;
        _recycleCount = 0;
    }

    /// <summary>
    /// Prepara o reel para parar, definindo os símbolos finais e ciclos extras
    /// Chamado pelo SpinController durante o pré-stop
    /// </summary>
    public void PrepareStop(SymbolSystem[] finalSymbols, int extraCycles)
    {
        _finalSymbols = finalSymbols;
        _stopRecycleCount = _recycleCount + extraCycles;
        _isPreparingStop = true;
    }

    private void Update()
    {
        if (!_isSpinning)
            return;

        MoveReel();
    }

    /// <summary>
    /// Move todas as células para baixo e verifica necessidade de reciclagem
    /// </summary>
    private void MoveReel()
    {
        foreach (PoolSystem cell in cells)
        {
            RectTransform rt = cell.GetComponent<RectTransform>();
            rt.anchoredPosition += Vector2.down * spinSpeed * Time.deltaTime;
        }

        PoolSystem bottomCell = cells[cells.Length - 1];
        RectTransform bottomRect = bottomCell.GetComponent<RectTransform>();

        if (bottomRect.anchoredPosition.y < recyclePosition)
        {
            HandleRecycle(bottomCell);
        }
    }

    /// <summary>
    /// Recicla a célula que saiu da tela, reposicionando no topo
    /// </summary>
    private void HandleRecycle(PoolSystem recycledCell)
    {
        _recycleCount++;

        RectTransform recycledRect = recycledCell.GetComponent<RectTransform>();
        RectTransform topRect = cells[0].GetComponent<RectTransform>();

        recycledRect.anchoredPosition = new Vector2(
            recycledRect.anchoredPosition.x,
            topRect.anchoredPosition.y + symbolSpacing);

        SymbolSystem symbol = _rngService.GetVisualSymbol();
        recycledCell.ShowSymbol(symbol);

        RotateArray();
        CheckStopCondition();
    }

    /// <summary>
    /// Verifica se o reel deve parar baseado na contagem de reciclagens
    /// </summary>
    private void CheckStopCondition()
    {
        if (!_isPreparingStop)
            return;

        if (_recycleCount >= _stopRecycleCount)
        {
            StopSpin();
        }
    }

    /// <summary>
    /// Para o movimento do reel, alinha as células e aplica os símbolos finais
    /// </summary>
    private void StopSpin()
    {
        _isSpinning = false;
        AlignCells();
        ApplyFinalSymbolsToVisibleCells();
        _onReelStopped?.Invoke();
    }

    /// <summary>
    /// Retorna as 3 células visíveis atualmente (topo, meio, baixo)
    /// Chamado pelo PaylineDrawer para desenhar as linhas de pagamento
    /// </summary>
    public PoolSystem[] GetVisibleCells()
    {
        PoolSystem[] visibleCells = new PoolSystem[3];

        for (int i = 0; i < visibleCells.Length; i++)
        {
            visibleCells[i] = null;
        }

        foreach (var cell in cells)
        {
            if (cell == null) continue;
            if (cell.GetComponent<RectTransform>() == null) continue;

            float y = cell.GetComponent<RectTransform>().anchoredPosition.y;

            if (Mathf.Abs(y - 170f) < 5f)
            {
                visibleCells[0] = cell;
            }
            else if (Mathf.Abs(y - 0f) < 5f)
            {
                visibleCells[1] = cell;
            }
            else if (Mathf.Abs(y + 170f) < 5f)
            {
                visibleCells[2] = cell;
            }
        }

        return visibleCells;
    }

    /// <summary>
    /// Alinha as células nas posições corretas após o stop
    /// </summary>
    private void AlignCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            RectTransform rt = cells[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, validPositions[i]);
        }
    }

    /// <summary>
    /// Rotaciona o array de células simulando o movimento circular
    /// </summary>
    private void RotateArray()
    {
        PoolSystem last = cells[cells.Length - 1];

        for (int i = cells.Length - 1; i > 0; i--)
        {
            cells[i] = cells[i - 1];
        }

        cells[0] = last;
    }

    /// <summary>
    /// Aplica os símbolos finais às células visíveis
    /// Chamado pelo StopSpin após o alinhamento
    /// </summary>
    private void ApplyFinalSymbolsToVisibleCells()
    {
        PoolSystem[] visibleCells = GetVisibleCells();

        if (visibleCells[0] == null || visibleCells[1] == null || visibleCells[2] == null)
        {
            Debug.LogError("VisibleCells inválido");
            return;
        }

        visibleCells[0].ShowSymbol(_finalSymbols[0]);
        visibleCells[1].ShowSymbol(_finalSymbols[1]);
        visibleCells[2].ShowSymbol(_finalSymbols[2]);
    }
}