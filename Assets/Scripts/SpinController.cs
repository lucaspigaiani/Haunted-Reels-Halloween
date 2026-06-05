using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpinController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button spinButton;
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

    private void Start()
    {
        _credits = startingCredits;
        _currentBet = minBet;

        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].Initialize(rngService);
        }

        UpdateUI();
    }

    public void StartSpin()
    {
        if (_isSpinning)
            return;

        if (!CanBet())
            return;

        _isSpinning = true;

        spinButton.interactable = false;

        _credits -= _currentBet;

        UpdateUI();

        // Resultado é definido ANTES da animação.
        _currentSpinResult = rngService.GenerateSpinResult();

        //TODO: remover
        rngService.LogSpinResult(_currentSpinResult);

        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StartSpin(OnReelStopped);
        }

        StartCoroutine(PrepareStopRoutine());
    }

    private int _stoppedReels;
    [SerializeField]private PaylineDebug paylineDebug;

    private void OnReelStopped()
    {
        _stoppedReels++;

        if (_stoppedReels < reels.Length)
            return;

        _stoppedReels = 0;

        FinishSpin();
    }

    private void FinishSpin()
    {
        var result = paylineSystem.Evaluate( _currentSpinResult, _currentBet);
        paylineDebug.ShowPaylines(_currentSpinResult);

        PayWin(result.TotalWin);

        winText.text = result.TotalWin.ToString();

        _isSpinning = false;

        spinButton.interactable = true;

        if (autoPlay)
        {
            StartCoroutine(AutoPlayRoutine());
        }
    }

    private IEnumerator PrepareStopRoutine()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].PrepareStop(
                GetReelResult(i),
                5 + i * 2
            );
        }
    }

    private SymbolSystem[] GetReelResult(int reelIndex)
    {
        return new SymbolSystem[]
        {
        _currentSpinResult.Grid[reelIndex, 0],
        _currentSpinResult.Grid[reelIndex, 1],
        _currentSpinResult.Grid[reelIndex, 2]
        };

    }

    private IEnumerator AutoPlayRoutine()
    {
        yield return new WaitForSeconds(autoPlayDelay);

        if (!autoPlay)
            yield break;

        if (_isSpinning)
            yield break;

        if (!CanBet())
            yield break;

        StartSpin();
    }

    public void AutoPlay()
    {
        if (autoPlay == false)
        {
            autoPlay = true;

            if (!_isSpinning && CanBet())
            {
                StartSpin();
            }
        }
        else if (autoPlay == true)
        {
            autoPlay = false;
        }
    }

    public void ToggleAutoPlay()
    {
        autoPlay = !autoPlay;

        if (autoPlay &&
            !_isSpinning &&
            CanBet())
        {
            StartSpin();
        }
    }

    public void IncreaseBet()
    {
        _currentBet += betStep;

        if (_currentBet > maxBet)
            _currentBet = maxBet;

        UpdateUI();
    }

    public void DecreaseBet()
    {
        _currentBet -= betStep;

        if (_currentBet < minBet)
            _currentBet = minBet;

        UpdateUI();
    }

    public bool CanBet()
    {
        return _credits >= _currentBet;
    }

    public void AddCredits(int amount)
    {
        _credits += amount;

        UpdateUI();
    }

    public void PayWin(float amount)
    {
        if (amount <= 0)
            return;

        _credits += amount;

        UpdateUI();
    }

    private void UpdateUI()
    {
        creditsText.text = _credits.ToString();
        betText.text = _currentBet.ToString();
    }
}