using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpinController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button autoPlayButton;
    [SerializeField] private Button increaseBetButton;
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private TMP_Text betText;
    [SerializeField] private TMP_Text winText;

    [Header("Systems")]
    [SerializeField] private RNGService rngService;
    [SerializeField] private PaylineSystem paylineSystem;
    [SerializeField] private ReelController[] reels;

    [Header("Player")]
    [SerializeField] private int startingCredits = 1000;
    [SerializeField] private int minBet = 10;
    [SerializeField] private int maxBet = 100;
    [SerializeField] private int betStep = 10;

    [Header("Auto Play")]
    [SerializeField] private bool autoPlay;
    [SerializeField] private float autoPlayDelay = 1.5f;

    private float _credits;
    private int _currentBet;
    private bool _isSpinning;
    private SpinResult _currentSpinResult;
    private int _stoppedReels;

    private void Start()
    {
        _credits = startingCredits;
        _currentBet = minBet;

        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].Initialize(rngService);
        }

        UpdateUI();
        winText.text = FormatMoney(0f);
        UpdateButtonsState();
    }

    /// <summary>
    /// Atualiza o estado dos botões baseado nas condições atuais
    /// </summary>
    private void UpdateButtonsState()
    {
        bool canChangeBet = !_isSpinning && !autoPlay;

        if (increaseBetButton != null)
            increaseBetButton.interactable = canChangeBet;

        if (autoPlayButton != null)
            autoPlayButton.interactable = !_isSpinning;

        if (spinButton != null)
            spinButton.interactable = !_isSpinning && CanBet();
    }

    /// <summary>
    /// Inicia uma nova rodada de spin
    /// Chamado pelo botão Spin ou pelo AutoPlay
    /// </summary>
    public void StartSpin()
    {
        if (_isSpinning)
            return;

        if (!CanBet())
            return;

        paylineSystem.StopPaylineDisplay();

        _isSpinning = true;
        UpdateButtonsState();

        _credits -= _currentBet;
        UpdateUI();

        _currentSpinResult = rngService.GenerateSpinResult();

        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StartSpin(OnReelStopped);
        }

        StartCoroutine(PrepareStopRoutine());
    }

    /// <summary>
    /// Callback chamado por cada ReelController quando o rolo para
    /// </summary>
    private void OnReelStopped()
    {
        _stoppedReels++;

        if (_stoppedReels < reels.Length)
            return;

        _stoppedReels = 0;
        FinishSpin();
    }

    /// <summary>
    /// Finaliza o spin, calcula prêmios e atualiza UI
    /// </summary>
    private void FinishSpin()
    {
        var result = paylineSystem.Evaluate(_currentSpinResult, _currentBet);
        PayWin(result.TotalWin);
        winText.text = FormatMoney(result.TotalWin);

        _isSpinning = false;
        UpdateButtonsState();

        if (autoPlay)
        {
            StartCoroutine(AutoPlayRoutine());
        }
    }

    /// <summary>
    /// Aguarda um tempo e prepara cada rolo para parar
    /// </summary>
    private IEnumerator PrepareStopRoutine()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].PrepareStop(GetReelResult(i), 5 + i * 2);
        }
    }

    /// <summary>
    /// Retorna os símbolos finais de um rolo específico
    /// </summary>
    private SymbolSystem[] GetReelResult(int reelIndex)
    {
        return new SymbolSystem[]
        {
            _currentSpinResult.Grid[reelIndex, 0],
            _currentSpinResult.Grid[reelIndex, 1],
            _currentSpinResult.Grid[reelIndex, 2]
        };
    }

    /// <summary>
    /// Executa spins automáticos em sequência
    /// </summary>
    private IEnumerator AutoPlayRoutine()
    {
        yield return new WaitForSeconds(autoPlayDelay);

        if (!autoPlay)
            yield break;

        if (_isSpinning)
            yield break;

        if (!CanBet())
        {
            autoPlay = false;
            UpdateButtonsState();
            yield break;
        }

        StartSpin();
    }

    /// <summary>
    /// Alterna o modo Auto Play
    /// Chamado pelo botão Auto Play via Inspector
    /// </summary>
    public void ToggleAutoPlay()
    {
        if (_isSpinning)
            return;

        autoPlay = !autoPlay;
        UpdateButtonsState();

        if (autoPlayButton != null)
        {
            var btnText = autoPlayButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
            {
                btnText.text = autoPlay ? "AUTO ON" : "AUTO OFF";
            }
        }

        if (autoPlay && !_isSpinning && CanBet())
        {
            StartSpin();
        }
    }

    /// <summary>
    /// Aumenta o valor da aposta
    /// Chamado pelo botão + via Inspector
    /// </summary>
    public void IncreaseBet()
    {
        if (_isSpinning || autoPlay)
            return;

        _currentBet += betStep;

        if (_currentBet > maxBet)
            _currentBet = maxBet;

        UpdateUI();
    }

    /// <summary>
    /// Verifica se o jogador tem créditos suficientes para a aposta atual
    /// </summary>
    public bool CanBet()
    {
        return _credits >= _currentBet;
    }

    /// <summary>
    /// Adiciona créditos ao jogador
    /// Chamado externamente para recarga
    /// </summary>
    public void AddCredits(int amount)
    {
        _credits += amount;
        UpdateUI();
        UpdateButtonsState();
    }

    /// <summary>
    /// Paga o prêmio ao jogador
    /// Chamado pelo FinishSpin após avaliação das paylines
    /// </summary>
    public void PayWin(float amount)
    {
        if (amount <= 0)
            return;

        _credits += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        creditsText.text = FormatMoney(_credits);
        betText.text = FormatMoney(_currentBet);
    }

    private string FormatMoney(float value)
    {
        return value.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    }

    private string FormatMoney(int value)
    {
        return value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    }
}