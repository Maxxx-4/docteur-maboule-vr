using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Chef d'orchestre : score, erreurs, minuteur, victoire / défaite.
/// À placer une seule fois dans la scène. Les UnityEvents se branchent dans l'Inspector
/// (ex. mettre à jour un texte TextMeshPro sur un Canvas en World Space).
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum State { Ready, Playing, Won, Lost }

    public static GameManager Instance { get; private set; }

    [Header("Règles")]
    [SerializeField] int totalPieces = 5;
    [SerializeField] int maxErrors = 3;
    [SerializeField] float timeLimitSeconds = 120f;
    [SerializeField] bool autoStart = true;

    [Header("Événements (UI, sons...)")]
    public UnityEvent<int> onScoreChanged;
    public UnityEvent<int> onErrorsChanged;
    public UnityEvent<float> onTimeChanged;
    public UnityEvent onWin;
    public UnityEvent onLose;

    public State CurrentState { get; private set; } = State.Ready;
    public int Score { get; private set; }
    public int Errors { get; private set; }
    public float TimeLeft { get; private set; }

    int piecesRemoved;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (autoStart) StartGame();
    }

    public void StartGame()
    {
        Score = 0; Errors = 0; piecesRemoved = 0;
        TimeLeft = timeLimitSeconds;
        CurrentState = State.Playing;
        onScoreChanged?.Invoke(Score);
        onErrorsChanged?.Invoke(Errors);
        onTimeChanged?.Invoke(TimeLeft);
    }

    void Update()
    {
        if (CurrentState != State.Playing) return;

        TimeLeft -= Time.deltaTime;
        onTimeChanged?.Invoke(Mathf.Max(0f, TimeLeft));
        if (TimeLeft <= 0f) Lose();
    }

    public void RegisterPieceRemoved(int points)
    {
        if (CurrentState != State.Playing) return;
        Score += points;
        piecesRemoved++;
        onScoreChanged?.Invoke(Score);
        if (piecesRemoved >= totalPieces) Win();
    }

    public void RegisterError()
    {
        if (CurrentState != State.Playing) return;
        Errors++;
        onErrorsChanged?.Invoke(Errors);
        if (Errors >= maxErrors) Lose();
    }

    void Win()  { CurrentState = State.Won;  onWin?.Invoke(); }
    void Lose() { CurrentState = State.Lost; onLose?.Invoke(); }
}
