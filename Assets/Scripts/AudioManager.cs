using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip evaluateMusic;    // Música durante avaliação
    [SerializeField] private AudioClip winMusic;        // Música de vitória

    [Header("SFX Clips")]
    [SerializeField] private AudioClip spinSfx;         // Som de giro da roleta
    [SerializeField] private AudioClip reelStopSfx;     // Som de parada do rolo (opcional)
    [SerializeField] private AudioClip payoutSfx;       // Som de pagamento
    [SerializeField] private AudioClip buttonClickSfx;  // Som de clique de botão

    private static AudioManager _instance;

    private void Awake()
    {
        // Singleton pattern para acesso fácil
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Toca música de fundo durante a avaliação das paylines
    /// Chamado pelo PaylineSystem.Evaluate()
    /// </summary>
    public void PlayEvaluateMusic()
    {
        if (musicSource == null || evaluateMusic == null) return;

        musicSource.clip = evaluateMusic;
        musicSource.Play();
    }

    /// <summary>
    /// Toca música de vitória
    /// Chamado pelo PaylineSystem quando TotalWin > 0
    /// </summary>
    public void PlayWinMusic()
    {
        if (musicSource == null || winMusic == null) return;

        musicSource.clip = winMusic;
        musicSource.Play();
    }

    /// <summary>
    /// Toca som de giro da roleta (loop)
    /// Chamado pelo SpinController.StartSpin() quando os rolos começam a girar
    /// </summary>
    public void PlaySpinSfx()
    {
        if (sfxSource == null || spinSfx == null) return;

        sfxSource.clip = spinSfx;
        sfxSource.loop = true;
        sfxSource.Play();
    }

    /// <summary>
    /// Para o som de giro da roleta
    /// Chamado pelo SpinController.FinishSpin() quando os rolos param
    /// </summary>
    public void StopSpinSfx()
    {
        if (sfxSource == null) return;

        sfxSource.loop = false;
        sfxSource.Stop();
    }

    /// <summary>
    /// Toca som de parada do rolo
    /// Chamado por cada ReelController quando para individualmente
    /// </summary>
    public void PlayReelStopSfx()
    {
        if (reelStopSfx == null) return;

        AudioSource.PlayClipAtPoint(reelStopSfx, Vector3.zero);
    }

    /// <summary>
    /// Toca som de pagamento (payout)
    /// Chamado pelo PaylineSystem quando há vitória
    /// </summary>
    public void PlayPayoutSfx()
    {
        if (payoutSfx == null) return;

        AudioSource.PlayClipAtPoint(payoutSfx, Vector3.zero);
    }

    /// <summary>
    /// Toca som de clique de botão
    /// Chamado pelos botões de UI (Spin, AutoPlay, +, -)
    /// </summary>
    public void PlayButtonClickSfx()
    {
        if (buttonClickSfx == null) return;

        AudioSource.PlayClipAtPoint(buttonClickSfx, Vector3.zero);
    }

    /// <summary>
    /// Para a música atual
    /// </summary>
    public void StopMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
    }

    // Métodos estáticos para acesso fácil de qualquer lugar
    public static void PlayEvaluate() => _instance?.PlayEvaluateMusic();
    public static void PlayWin() => _instance?.PlayWinMusic();
    public static void PlaySpin() => _instance?.PlaySpinSfx();
    public static void StopSpin() => _instance?.StopSpinSfx();
    public static void PlayReelStop() => _instance?.PlayReelStopSfx();
    public static void PlayPayout() => _instance?.PlayPayoutSfx();
    public static void PlayButtonClick() => _instance?.PlayButtonClickSfx();
    public static void StopMusicAll() => _instance?.StopMusic();
}