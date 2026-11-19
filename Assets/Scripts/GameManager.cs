using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Chef d'orchestre : score, erreurs, minuteur, victoire / défaite, classement.
/// À placer une seule fois dans la scène. Les autres scripts (BodyPart, CavityEdge, ScoreboardUI...)
/// lui parlent via GameManager.Instance.
///
/// Score final (victoire) = points des pièces + bonus de temps (secondes restantes x timeBonusPerSecond)
///                          - pénalité d'erreurs. Le temps mis pour tout retirer est enregistré dans le classement.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum State { Ready, Playing, Won, Lost }

    public static GameManager Instance { get; private set; }

    [Header("Règles")]
    [SerializeField] int totalPieces = 5;
    [SerializeField] int maxErrors = 3;
    [SerializeField] float timeLimitSeconds = 120f;
    [SerializeField] bool autoStart = true;   // false : c'est le panneau des règles (RulesPanel) qui lance la partie

    [Header("Score")]
    [SerializeField] int timeBonusPerSecond = 5;
    [SerializeField] int errorPenalty = 50;

    [Header("Rejouer")]
    [SerializeField] float restartDelay = 1.5f;   // délai avant que A / X / Entrée relance la partie

    public State CurrentState { get; private set; } = State.Ready;
    public int Score { get; private set; }
    public int Errors { get; private set; }
    public float TimeLeft { get; private set; }
    public float ElapsedTime { get; private set; }
    public int PiecesRemoved { get; private set; }
    public int LeaderboardRank { get; private set; } = -1;   // 0 = premier, -1 = hors Top 10

    // Réglages en lecture seule (affichés par ScoreboardUI et RulesPanel)
    public int TotalPieces => totalPieces;
    public int MaxErrors => maxErrors;
    public float TimeLimitSeconds => timeLimitSeconds;
    public int TimeBonusPerSecond => timeBonusPerSecond;
    public int ErrorPenalty => errorPenalty;

    float endTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (autoStart) StartGame();
    }

    /// <summary>Remet tout à zéro et lance le chrono.</summary>
    public void StartGame()
    {
        Score = 0; Errors = 0; PiecesRemoved = 0; ElapsedTime = 0f; endTimer = 0f;
        LeaderboardRank = -1;
        TimeLeft = timeLimitSeconds;
        CurrentState = State.Playing;
    }

    void Update()
    {
        if (CurrentState == State.Playing)
        {
            TimeLeft -= Time.deltaTime;
            ElapsedTime += Time.deltaTime;
            if (TimeLeft <= 0f) { TimeLeft = 0f; Lose(); }
        }
        else if (CurrentState == State.Won || CurrentState == State.Lost)
        {
            // fin de partie : après un court délai, A / X / Entrée relance
            endTimer += Time.unscaledDeltaTime;
            if (endTimer > restartDelay && RestartPressed()) Restart();
        }
    }

    /// <summary>Appelé par BodyPart quand une pièce est sortie de sa cavité.</summary>
    public void RegisterPieceRemoved(int points)
    {
        if (CurrentState != State.Playing) return;
        Score += points;
        PiecesRemoved++;
        if (PiecesRemoved >= totalPieces) Win();
    }

    /// <summary>Appelé par CavityEdge quand la pince touche un bord.</summary>
    public void RegisterError()
    {
        if (CurrentState != State.Playing) return;
        Errors++;
        if (Errors >= maxErrors) Lose();
    }

    void Win()
    {
        CurrentState = State.Won;
        Score = Mathf.Max(0, Score + Mathf.RoundToInt(TimeLeft * timeBonusPerSecond) - Errors * errorPenalty);
        LeaderboardRank = Leaderboard.Add(ElapsedTime, Errors, Score, out _);
        if (AlertFeedback.Instance != null) AlertFeedback.Instance.PlayWin();
    }

    void Lose()
    {
        CurrentState = State.Lost;
        if (AlertFeedback.Instance != null) AlertFeedback.Instance.PlayLose();
    }

    /// <summary>Recharge la scène (le panneau des règles ne réapparaît pas).</summary>
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Quest : bouton A/X d'une manette. PC / simulateur : touche Entrée.
    static bool RestartPressed()
    {
        if (ButtonDown(XRNode.RightHand) || ButtonDown(XRNode.LeftHand)) return true;
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
#else
        return false;
#endif
    }

    static bool ButtonDown(XRNode node)
    {
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        return device.isValid &&
               device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool pressed) && pressed;
    }
}
