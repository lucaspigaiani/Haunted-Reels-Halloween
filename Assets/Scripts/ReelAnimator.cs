using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ReelAnimator : MonoBehaviour
{
    private readonly List<SkeletonGraphic> activeSpines = new();

    public void RegisterSpine(SkeletonGraphic spine)
    {
        if (!activeSpines.Contains(spine))
        {
            activeSpines.Add(spine);
        }
    }

    public void RemoveSpine(SkeletonGraphic spine)
    {
        activeSpines.Remove(spine);
    }

    public void PlayIdle()
    {
        foreach (var spine in activeSpines)
        {
            spine.AnimationState.SetAnimation(
                0,
                "idle",
                true);
        }
    }

    public void PlayNone()
    {
        foreach (var spine in activeSpines)
        {
            spine.AnimationState.SetAnimation(
                0,
                "none",
                true);
        }
    }
}