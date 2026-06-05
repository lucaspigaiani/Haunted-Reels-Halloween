using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PaylineSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ReelAnimator reelAnimator;
    [SerializeField] private PaylineDrawer paylineDrawer;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip evaluateMusic;
    [SerializeField] private AudioClip winMusic;
    [SerializeField] private AudioClip payoutSfx;

    private readonly int[,] paylines = new int[,]
    {
        {1,1,1,1,1}, {0,0,0,0,0}, {2,2,2,2,2},
        {2,1,0,1,2}, {0,1,2,1,0}, {0,0,1,2,2},
        {2,2,1,0,0}, {0,1,1,2,2}, {2,1,1,0,0},
        {0,1,1,1,2}
    };

    private PaylineCalculator calculator;

    private void Awake()
    {
        calculator = new PaylineCalculator(paylines);

        if (paylineDrawer == null)
            paylineDrawer = GetComponent<PaylineDrawer>();

        paylineDrawer.SetPaylines(paylines);
    }

    public PaylineResult Evaluate(SpinResult spinResult, int totalBet)
    {
        PlayEvaluateMusic();

        PaylineResult result = new PaylineResult();
        float lineBet = totalBet / 10f;

        List<PaylineEvaluation> evaluations = calculator.EvaluateAllLines(spinResult, lineBet);

        float totalWin = 0f;
        foreach (var eval in evaluations)
        {
            if (eval.IsWinning)
            {
                totalWin += eval.Payout;
                result.WinningLines.Add(eval.LineIndex);
            }
        }
        result.TotalWin = totalWin;

        GenerateDebugLog(evaluations, totalBet, lineBet, totalWin);

        if (paylineDrawer != null)
            paylineDrawer.DrawPaylinesSequentially(evaluations);

        if (result.TotalWin > 0)
            PlayWinEffects(result);

        return result;
    }

    private void GenerateDebugLog(List<PaylineEvaluation> evaluations, int totalBet, float lineBet, float totalWin)
    {
        StringBuilder debugLog = new StringBuilder();
        debugLog.AppendLine("========== AVALIACAO DE PAYLINES ==========");
        debugLog.AppendLine($"Aposta total: {totalBet} | Aposta por linha: {lineBet:F2}");
        debugLog.AppendLine("----------------------------------------");

        foreach (var eval in evaluations)
        {
            if (eval.IsWinning)
            {
                debugLog.AppendLine($"[POSITIVO] LINHA {eval.LineIndex + 1,2} | {eval.TotalCount}x {eval.SymbolName} | " +
                    $"{eval.BestCount} simb + {eval.WildCount} wild = {eval.TotalCount} | " +
                    $"{eval.Multiplier:F2}x{lineBet:F2} = {eval.Payout:F2} | Simbolos: {eval.SymbolsString}");
            }
            else
            {
                debugLog.AppendLine($"[NEGATIVO] LINHA {eval.LineIndex + 1,2} | " +
                    $"Melhor: {eval.BestCount}x {eval.BestSymbolType} + {eval.WildCount}x wild = {eval.TotalCount}x (minimo 3) | " +
                    $"Simbolos: {eval.SymbolsString}");
            }
        }

        debugLog.AppendLine("----------------------------------------");
        debugLog.AppendLine($"TOTAL GANHO: {totalWin:F2} creditos");
        debugLog.AppendLine("=========================================");

        Debug.Log(debugLog.ToString());
    }

    public void StopPaylineDisplay()
    {
        if (paylineDrawer != null)
            paylineDrawer.StopDrawing();
    }

    private void PlayEvaluateMusic()
    {
        if (musicSource == null || evaluateMusic == null) return;
        musicSource.clip = evaluateMusic;
        musicSource.Play();
    }

    private void PlayWinEffects(PaylineResult result)
    {
        if (musicSource != null && winMusic != null)
        {
            musicSource.clip = winMusic;
            musicSource.Play();
        }

        if (payoutSfx != null)
            AudioSource.PlayClipAtPoint(payoutSfx, Vector3.zero);

        if (reelAnimator != null)
            reelAnimator.PlayIdle();
    }
}