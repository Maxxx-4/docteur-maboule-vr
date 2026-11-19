using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tableau d'affichage en World Space : temps, erreurs, pièces, score + classement Top 5.
/// Lit directement GameManager (aucun câblage d'événements à faire).
/// </summary>
public class ScoreboardUI : MonoBehaviour
{
    [SerializeField] Text statsText;
    [SerializeField] Text bannerText;
    [SerializeField] Text leaderboardText;
    [SerializeField] Text hintText;
    [SerializeField] int leaderboardLines = 5;

    GameManager.State lastState = (GameManager.State)(-1);
    float nextRefresh;
    readonly StringBuilder sb = new StringBuilder();

    void Awake()
    {
        // Clic souris dans la vue Game (test sur PC) : le Canvas a besoin d'une caméra d'événements.
        var canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null) canvas.worldCamera = Camera.main;
    }

    void Start()
    {
        RefreshLeaderboard();
    }

    /// <summary>Branché sur le bouton "REJOUER" du tableau.</summary>
    public void RestartGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.Restart();
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.CurrentState != lastState)
        {
            lastState = gm.CurrentState;
            RefreshLeaderboard();
            RefreshBanner(gm);
        }

        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.1f;

        sb.Length = 0;
        sb.Append("TEMPS     ").Append(FormatTime(gm.ElapsedTime)).Append('\n');
        sb.Append("RESTANT   ").Append(FormatTime(Mathf.Max(0f, gm.TimeLeft))).Append('\n');
        sb.Append("ERREURS   ").Append(gm.Errors).Append(" / ").Append(gm.MaxErrors).Append('\n');
        sb.Append("PIÈCES    ").Append(gm.PiecesRemoved).Append(" / ").Append(gm.TotalPieces).Append('\n');
        sb.Append("SCORE     ").Append(gm.Score);
        if (statsText != null) statsText.text = sb.ToString();
    }

    void RefreshBanner(GameManager gm)
    {
        if (bannerText == null) return;
        switch (gm.CurrentState)
        {
            case GameManager.State.Won:
                bannerText.color = new Color(0.4f, 1f, 0.4f);
                bannerText.text = "VICTOIRE ! " + FormatTime(gm.ElapsedTime) + " — " + gm.Score + " pts" +
                                  (gm.LeaderboardRank >= 0 ? "  (rang #" + (gm.LeaderboardRank + 1) + ")" : "");
                break;
            case GameManager.State.Lost:
                bannerText.color = new Color(1f, 0.3f, 0.3f);
                bannerText.text = gm.Errors >= gm.MaxErrors ? "TROP D'ERREURS... PERDU !" : "TEMPS ÉCOULÉ... PERDU !";
                break;
            default:
                bannerText.color = new Color(1f, 0.9f, 0.3f);
                bannerText.text = "Retire toutes les pièces sans toucher les bords !";
                break;
        }

        if (hintText != null)
        {
            bool ended = gm.CurrentState == GameManager.State.Won || gm.CurrentState == GameManager.State.Lost;
            hintText.text = ended ? "Clique sur REJOUER (ou bouton A / X, Entrée)" : "";
        }
    }

    void RefreshLeaderboard()
    {
        if (leaderboardText == null) return;
        var list = Leaderboard.Load();
        GameManager gm = GameManager.Instance;
        int myRank = gm != null && gm.CurrentState == GameManager.State.Won ? gm.LeaderboardRank : -1;

        sb.Length = 0;
        sb.Append("<b>CLASSEMENT</b>\n");
        if (list.Count == 0) sb.Append("Aucun score pour l'instant.");
        for (int i = 0; i < list.Count && i < leaderboardLines; i++)
        {
            LeaderboardEntry e = list[i];
            string line = (i + 1) + ".  " + e.name + "   " + FormatTime(e.time) + "   " + e.score + " pts";
            if (i == myRank) sb.Append("<color=#FFD700>").Append(line).Append("</color>");
            else sb.Append(line);
            if (i < list.Count - 1 && i < leaderboardLines - 1) sb.Append('\n');
        }
        leaderboardText.text = sb.ToString();
    }

    static string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60f);
        float s = seconds - m * 60f;
        return m + ":" + s.ToString("00.0", CultureInfo.InvariantCulture);
    }
}
