using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ReelAnimator : MonoBehaviour
{
    private readonly List<SkeletonGraphic> activeSpines = new();

    /// <summary>
    /// Registra uma animação Spine para ser controlada
    /// Chamado pelo PoolSystem quando um símbolo Spine é exibido
    /// </summary>
    public void RegisterSpine(SkeletonGraphic spine)
    {
        if (!activeSpines.Contains(spine))
        {
            activeSpines.Add(spine);
        }
    }

    /// <summary>
    /// Remove uma animação Spine do controle
    /// Chamado pelo PoolSystem quando um símbolo Spine é substituído por imagem estática
    /// </summary>
    public void RemoveSpine(SkeletonGraphic spine)
    {
        activeSpines.Remove(spine);
    }

    /// <summary>
    /// Reproduz animação "idle" em todos os símbolos Spine ativos
    /// Chamado pelo PaylineSystem após uma vitória para manter animação suave
    /// </summary>
    public void PlayIdle()
    {
        foreach (var spine in activeSpines)
        {
            spine.AnimationState.SetAnimation(0, "idle", true);
        }
    }

    /// <summary>
    /// Para todas as animações dos símbolos Spine
    /// </summary>
    public void PlayNone()
    {
        foreach (var spine in activeSpines)
        {
            spine.AnimationState.SetAnimation(0, "none", true);
        }
    }
}