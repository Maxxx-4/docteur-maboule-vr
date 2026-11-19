using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Une ligne du classement.</summary>
[Serializable]
public class LeaderboardEntry
{
    public string name;
    public float time;     // secondes mises pour retirer toutes les pièces
    public int errors;
    public int score;
    public string date;
}

[Serializable]
class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

/// <summary>
/// Classement local (Top 10) sauvegardé dans PlayerPrefs (fonctionne dans l'éditeur et sur le Quest).
/// Tri : score décroissant, puis temps croissant.
/// </summary>
public static class Leaderboard
{
    const string Key = "dm_leaderboard_v1";
    const string CounterKey = "dm_player_counter";   // sert à nommer "Joueur 1", "Joueur 2"...
    public const int MaxEntries = 10;

    public static List<LeaderboardEntry> Load()
    {
        string json = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrEmpty(json)) return new List<LeaderboardEntry>();
        try { return JsonUtility.FromJson<LeaderboardData>(json)?.entries ?? new List<LeaderboardEntry>(); }
        catch (Exception) { return new List<LeaderboardEntry>(); }   // sauvegarde corrompue : on repart de zéro
    }

    /// <summary>Ajoute une partie gagnée. Retourne le rang (0 = premier) ou -1 si hors du Top 10.</summary>
    public static int Add(float time, int errors, int score, out string playerName)
    {
        int n = PlayerPrefs.GetInt(CounterKey, 0) + 1;
        PlayerPrefs.SetInt(CounterKey, n);
        playerName = "Joueur " + n;

        var entry = new LeaderboardEntry
        {
            name = playerName, time = time, errors = errors, score = score,
            date = DateTime.Now.ToString("dd/MM HH:mm")
        };

        var list = Load();
        list.Add(entry);
        list.Sort((a, b) => b.score != a.score ? b.score.CompareTo(a.score) : a.time.CompareTo(b.time));

        int rank = list.IndexOf(entry);
        if (list.Count > MaxEntries) list.RemoveRange(MaxEntries, list.Count - MaxEntries);

        PlayerPrefs.SetString(Key, JsonUtility.ToJson(new LeaderboardData { entries = list }));
        PlayerPrefs.Save();
        return rank >= MaxEntries ? -1 : rank;
    }

    /// <summary>Efface le classement.</summary>
    public static void Clear()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.DeleteKey(CounterKey);
        PlayerPrefs.Save();
    }
}
