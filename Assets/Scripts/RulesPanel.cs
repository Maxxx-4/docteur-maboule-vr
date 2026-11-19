using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Panneau des règles affiché au lancement, à droite du joueur. La partie ne démarre (et le chrono
/// ne tourne) qu'une fois le panneau fermé par un clic sur le bouton (rayon de la manette) ou avec Entrée.
/// Après un "Rejouer", le panneau n'est plus affiché : la partie redémarre directement.
/// </summary>
public class RulesPanel : MonoBehaviour
{
    [SerializeField] Text bodyText;

    static bool dismissedOnce;   // reste vrai pendant la session (rechargements de scène compris)

    void Awake()
    {
        // Clic souris dans la vue Game (test sur PC) : le Canvas a besoin d'une caméra d'événements.
        var canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null) canvas.worldCamera = Camera.main;
    }

    void Start()
    {
        GameManager gm = GameManager.Instance;
        if (dismissedOnce)
        {
            gameObject.SetActive(false);
            if (gm != null && gm.CurrentState == GameManager.State.Ready) gm.StartGame();
            return;
        }

        if (bodyText != null && gm != null)
        {
            int minutes = Mathf.RoundToInt(gm.TimeLimitSeconds / 60f);
            bodyText.text =
                "•  Attrape la pince avec la gâchette / le grip de la manette.\n" +
                "•  Retire les " + gm.TotalPieces + " objets des cavités de Gérard.\n" +
                "•  Ne touche jamais les bords : lumière rouge + buzzer = 1 erreur.\n" +
                "•  " + gm.MaxErrors + " erreurs ou " + minutes + " minutes écoulées : c'est perdu !\n" +
                "•  Score : 100 pts par objet, + " + gm.TimeBonusPerSecond + " pts par seconde restante, - " +
                gm.ErrorPenalty + " pts par erreur.\n" +
                "•  Les meilleurs scores entrent au classement.";
        }
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            Close();
#endif
    }

    /// <summary>Branché sur le bouton "COMPRIS". Ferme le panneau et lance la partie.</summary>
    public void Close()
    {
        dismissedOnce = true;
        gameObject.SetActive(false);
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.CurrentState == GameManager.State.Ready) gm.StartGame();
    }
}
