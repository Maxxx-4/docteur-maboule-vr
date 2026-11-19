using UnityEngine;

/// <summary>
/// Pièce à retirer (ex. cœur, os...). Elle n'est PAS saisie à la main : seule la pince peut la prendre
/// (voir PinceGrabber). Quand elle est assez loin de sa cavité, elle compte comme extraite.
/// </summary>
public class BodyPart : MonoBehaviour
{
    [SerializeField] int points = 100;
    [Tooltip("Distance (m) à partir de laquelle la pièce est considérée comme retirée.")]
    [SerializeField] float extractDistance = 0.15f;
    [SerializeField] AudioSource successSound;

    Vector3 homePosition;
    bool extracted;
    Rigidbody rb;

    /// <summary>Vrai tant que la pince tient la pièce.</summary>
    public bool IsCarried { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        homePosition = transform.position;
    }

    /// <summary>Appelé par la pince quand elle se ferme sur la pièce.</summary>
    public void PickUp()
    {
        IsCarried = true;
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
    }

    /// <summary>Appelé par la pince quand elle s'ouvre : la pièce tombe.</summary>
    public void Release()
    {
        IsCarried = false;
        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
    }

    void Update()
    {
        if (extracted) return;
        // Ne compte que pendant la partie (pas pendant l'écran des règles ni après la fin)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.State.Playing) return;
        if (Vector3.Distance(transform.position, homePosition) < extractDistance) return;

        extracted = true;
        if (successSound != null) successSound.Play();
        if (GameManager.Instance != null) GameManager.Instance.RegisterPieceRemoved(points);
    }
}
