using System.Collections.Generic;
using UnityEngine;

public class PaylineSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ReelAnimator reelAnimator;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioClip evaluateMusic;
    [SerializeField] private AudioClip winMusic;
    [SerializeField] private AudioClip payoutSfx;

    /*
     * 5 reels x 3 rows
     *
     * 0 = top
     * 1 = middle
     * 2 = bottom
     */

    private readonly int[,] paylines =
    {
        {1,1,1,1,1}, // Middle
        {0,0,0,0,0}, // Top
        {2,2,2,2,2}, // Bottom

        {0,1,2,1,0},
        {2,1,0,1,2},

        {0,0,1,0,0},
        {2,2,1,2,2},

        {1,0,0,0,1},
        {1,2,2,2,1},

        {0,1,1,1,0}
    };

    public PaylineResult Evaluate(
        SpinResult spinResult,
        int lineBet)
    {
        PlayEvaluateMusic();

        PaylineResult result = new();

        for (int lineIndex = 0; lineIndex < paylines.GetLength(0); lineIndex++)
        {
            EvaluateLine(
                spinResult,
                lineIndex,
                lineBet,
                result);
        }

        if (result.TotalWin > 0)
        {
            PlayWinEffects(result);
        }

        return result;
    }

    private void EvaluateLine(
        SpinResult spinResult,
        int lineIndex,
        int lineBet,
        PaylineResult result)
    {
        SymbolSystem firstSymbol = null;

        int matches = 0;

        for (int reel = 0; reel < 5; reel++)
        {
            int row = paylines[lineIndex, reel];

            SymbolSystem current =
                spinResult.Grid[reel, row];

            if (firstSymbol == null)
            {
                if (current.Type ==
                    SymbolSystem.SymbolType.Wild)
                {
                    matches++;
                    continue;
                }

                firstSymbol = current;
                matches++;
                continue;
            }

            if (current.Type == firstSymbol.Type)
            {
                matches++;
            }
            else if (
                current.Type ==
                SymbolSystem.SymbolType.Wild)
            {
                matches++;
            }
            else
            {
                break;
            }
        }

        if (matches < 3)
            return;

        float payout = CalculatePayout(firstSymbol, matches, lineBet);

        if (payout <= 0)
            return;

        result.TotalWin += payout;
        result.WinningLines.Add(lineIndex);
    }

    private float CalculatePayout(
        SymbolSystem symbol,
        int matches,
        int lineBet)
    {
        if (symbol == null)
            return 0;

        float multiplier = matches switch
        {
            3 => symbol.Multiplier3,
            4 => symbol.Multiplier4,
            5 => symbol.Multiplier5,
            _ => 0
        };

        return multiplier * lineBet;
    }

    private void PlayEvaluateMusic()
    {
        if (musicSource == null)
            return;

        if (evaluateMusic == null)
            return;

        musicSource.clip = evaluateMusic;
        musicSource.Play();
    }

    private void PlayWinEffects(
        PaylineResult result)
    {
        if (musicSource != null &&
            winMusic != null)
        {
            musicSource.clip = winMusic;
            musicSource.Play();
        }

        if (payoutSfx != null)
        {
            AudioSource.PlayClipAtPoint(
                payoutSfx,
                Vector3.zero);
        }

        reelAnimator.PlayIdle();
    }
}