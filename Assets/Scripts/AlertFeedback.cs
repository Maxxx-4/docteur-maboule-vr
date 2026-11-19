using System.Collections;
using UnityEngine;

/// <summary>
/// Retour d'erreur : quand la pince touche le bord d'une cavité, la lumière devient rouge
/// (clignotement de la lumière principale + lumière rouge d'alerte) et un buzzer retentit.
/// Les sons sont générés par code (aucun fichier audio nécessaire) : buzzer, jingle de victoire, buzzer de défaite.
/// </summary>
public class AlertFeedback : MonoBehaviour
{
    public static AlertFeedback Instance { get; private set; }

    [Header("Lumières")]
    [SerializeField] Light mainLight;      // la Directional Light de la scène
    [SerializeField] Light alertLight;     // point light rouge, désactivée par défaut
    [SerializeField] Color alertColor = new Color(1f, 0.05f, 0.05f);
    [SerializeField] float alertLightIntensity = 12f;
    [SerializeField] float flashDuration = 0.9f;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [Range(0f, 1f)] [SerializeField] float volume = 0.8f;

    AudioClip buzzerClip, loseClip, winClip;
    Color baseColor;
    float baseIntensity;
    Coroutine flash;

    void Awake()
    {
        Instance = this;

        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;   // son 2D, entendu partout

        if (mainLight != null) { baseColor = mainLight.color; baseIntensity = mainLight.intensity; }
        if (alertLight != null) alertLight.enabled = false;

        buzzerClip = MakeBuzzer(0.6f, 130f, 130f);
        loseClip = MakeBuzzer(1.4f, 160f, 70f);
        winClip = MakeChime();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Appelé par CavityEdge à chaque contact pince / bord (seulement pendant la partie).</summary>
    public void Trigger()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.State.Playing) return;
        audioSource.PlayOneShot(buzzerClip, volume);
        StartFlash(flashDuration, true);
    }

    /// <summary>Défaite : long buzzer + lumière rouge fixe.</summary>
    public void PlayLose()
    {
        audioSource.PlayOneShot(loseClip, volume);
        StartFlash(1.8f, false);
    }

    /// <summary>Victoire : petit jingle.</summary>
    public void PlayWin()
    {
        audioSource.PlayOneShot(winClip, volume);
    }

    void StartFlash(float duration, bool blink)
    {
        if (flash != null) StopCoroutine(flash);
        flash = StartCoroutine(Flash(duration, blink));
    }

    // La lumière passe au rouge puis revient à la normale (en clignotant si blink = true).
    IEnumerator Flash(float duration, bool blink)
    {
        if (alertLight != null) { alertLight.enabled = true; alertLight.color = alertColor; }

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;                                              // 0 -> 1
            float pulse = blink ? Mathf.Abs(Mathf.Sin(k * Mathf.PI * 3f)) : 1f;   // clignotement
            float strength = Mathf.Clamp01(pulse * (0.35f + 0.65f * (1f - k)));    // s'estompe avec le temps

            if (alertLight != null) alertLight.intensity = alertLightIntensity * strength;
            if (mainLight != null)
            {
                mainLight.color = Color.Lerp(baseColor, alertColor, strength);
                mainLight.intensity = Mathf.Lerp(baseIntensity, baseIntensity * 0.6f, strength);
            }
            yield return null;
        }

        // retour à la normale
        if (alertLight != null) alertLight.enabled = false;
        if (mainLight != null) { mainLight.color = baseColor; mainLight.intensity = baseIntensity; }
        flash = null;
    }

    // ---------- sons générés par code ----------

    // Buzzer : mélange d'onde carrée et de dent de scie, dont la fréquence glisse de startFreq à endFreq.
    static AudioClip MakeBuzzer(float seconds, float startFreq, float endFreq)
    {
        const int rate = 44100;
        int n = Mathf.RoundToInt(seconds * rate);
        var data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float k = (float)i / n;
            phase += Mathf.Lerp(startFreq, endFreq, k) / rate;
            float square = (phase % 1f) < 0.5f ? 1f : -1f;
            float saw = ((phase * 1.5f) % 1f) * 2f - 1f;
            float envelope = Mathf.Min(1f, i / 300f) * Mathf.Clamp01((1f - k) * 6f);   // attaque + fin douce
            data[i] = (square * 0.6f + saw * 0.4f) * 0.45f * envelope;
        }
        return ToClip("Buzzer", data, rate);
    }

    // Jingle de victoire : 4 notes montantes (do, mi, sol, do aigu) qui s'éteignent.
    static AudioClip MakeChime()
    {
        const int rate = 44100;
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
        int noteLen = Mathf.RoundToInt(0.16f * rate);
        var data = new float[noteLen * notes.Length + rate / 4];
        for (int j = 0; j < notes.Length; j++)
        {
            for (int i = 0; i < noteLen + rate / 4 && j * noteLen + i < data.Length; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Min(1f, i / 200f) * Mathf.Exp(-t * 6f);
                data[j * noteLen + i] += Mathf.Sin(2f * Mathf.PI * notes[j] * t) * 0.35f * envelope;
            }
        }
        return ToClip("Win", data, rate);
    }

    static AudioClip ToClip(string name, float[] data, int rate)
    {
        var clip = AudioClip.Create(name, data.Length, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
