using UnityEngine;

/// <summary>
/// Pièce à retirer (ex. cœur, os...). Elle doit être saisissable (XR Grab Interactable)
/// et avoir un Rigidbody. Quand elle est assez loin de sa cavité, elle compte comme extraite.
/// </summary>
public class BodyPart : MonoBehaviour
{
    [SerializeField] int points = 100;
    [Tooltip("Distance (m) à partir de laquelle la pièce est considérée comme retirée.")]
    [SerializeField] float extractDistance = 0.15f;
    [SerializeField] AudioSource successSound;

    Vector3 homePosition;
    bool extracted;

    void Start()
    {
        homePosition = transform.position;
    }

    void Update()
    {
        if (extracted) return;
        if (Vector3.Distance(transform.position, homePosition) < extractDistance) return;

        extracted = true;
        if (successSound != null) successSound.Play();
        if (GameManager.Instance != null) GameManager.Instance.RegisterPieceRemoved(points);
    }
}
