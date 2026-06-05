using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PoolSystem : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image imageSymbol;
    [SerializeField] private SkeletonGraphic spineSymbol;

    [Header("Animator")]
    [SerializeField] private ReelAnimator reelAnimator;

    private SymbolSystem currentSymbol;
    public SymbolSystem CurrentSymbol => currentSymbol;

    /// <summary>
    /// Exibe o símbolo na célula, escolhendo entre imagem estática ou animação Spine
    /// Chamado pelo ReelController durante o spin e ao aplicar símbolos finais
    /// </summary>
    public void ShowSymbol(SymbolSystem symbol)
    {
        currentSymbol = symbol;

        if (!string.IsNullOrEmpty(symbol.SpineSkin))
        {
            ShowSpine(symbol);
        }
        else
        {
            ShowImage(symbol);
        }
    }

    /// <summary>
    /// Exibe símbolo como imagem estática (símbolos de baixo valor)
    /// </summary>
    private void ShowImage(SymbolSystem symbol)
    {
        imageSymbol.gameObject.SetActive(true);
        spineSymbol.gameObject.SetActive(false);

        imageSymbol.sprite = symbol.Sprite;

        if (reelAnimator != null)
        {
            reelAnimator.RemoveSpine(spineSymbol);
        }
    }

    /// <summary>
    /// Exibe símbolo como animação Spine (símbolos de alto valor)
    /// </summary>
    private void ShowSpine(SymbolSystem symbol)
    {
        imageSymbol.gameObject.SetActive(false);
        spineSymbol.gameObject.SetActive(true);

        spineSymbol.initialSkinName = symbol.SpineSkin;
        spineSymbol.Initialize(true);

        if (reelAnimator != null)
        {
            reelAnimator.RegisterSpine(spineSymbol);
        }
    }
}