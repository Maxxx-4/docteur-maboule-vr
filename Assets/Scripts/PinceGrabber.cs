using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// La pince, comme dans le vrai jeu : ses deux branches sont écartées au repos. On serre la GÂCHETTE :
/// les branches se referment et pincent la poignée de l'objet situé entre les mâchoires (elles s'arrêtent
/// sur la poignée). On relâche la gâchette : les mâchoires s'ouvrent et l'objet tombe.
/// Lâcher la pince (grip) l'ouvre aussi. À placer sur la racine de la pince (avec XRGrabInteractable).
/// </summary>
public class PinceGrabber : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] Transform tipPoint;
    [SerializeField] Transform branchLeft;
    [SerializeField] Transform branchRight;

    [Header("Mâchoires")]
    [Tooltip("Angle (degrés) de chaque branche quand la pince est ouverte.")]
    [SerializeField] float openAngle = 7f;
    [Tooltip("Vitesse d'ouverture / fermeture (degrés par seconde).")]
    [SerializeField] float jawSpeed = 140f;
    [Tooltip("Longueur d'une branche (m), du pivot à la pointe.")]
    [SerializeField] float jawLength = 0.1165f;
    [Tooltip("Demi-écart entre les branches fermées (m).")]
    [SerializeField] float jawInnerOffset = 0.0013f;
    [Tooltip("Épaisseur de la poignée pincée (m) : les mâchoires s'arrêtent dessus.")]
    [SerializeField] float handleThickness = 0.008f;

    [Header("Saisie")]
    [Tooltip("Rayon de recherche autour de la pointe de la pince (m).")]
    [SerializeField] float searchRadius = 0.07f;
    [Tooltip("Distance max entre la pointe et la poignée de l'objet pour le pincer (m).")]
    [SerializeField] float grabDistance = 0.035f;
    [Tooltip("Durée (s) pendant laquelle l'objet glisse jusqu'au fond des mâchoires.")]
    [SerializeField] float pinchBlendTime = 0.12f;

    XRGrabInteractable grab;
    OperationTool tool;
    Quaternion restLeft, restRight;
    float jawAngle;          // angle actuel de chaque branche (degrés)
    bool closed;
    BodyPart carried;
    Vector3 startLocalPos, targetLocalPos;   // dans le repère de tipPoint
    Quaternion carriedLocalRot;
    float blend;
    float pinchedAngle;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        tool = GetComponent<OperationTool>();
        if (branchLeft != null) restLeft = branchLeft.localRotation;
        if (branchRight != null) restRight = branchRight.localRotation;
        jawAngle = openAngle;
        ApplyJaws();

        if (grab != null)
        {
            // "Activate" = gâchette de la manette qui tient la pince (clic gauche dans le simulateur)
            grab.activated.AddListener(_ => SetClosed(true));
            grab.deactivated.AddListener(_ => SetClosed(false));
            // lâcher la pince (grip relâché) l'ouvre aussi
            grab.selectExited.AddListener(_ => SetClosed(false));
        }
    }

    void Update()
    {
        float target = !closed ? openAngle : (carried != null ? pinchedAngle : 0f);
        jawAngle = Mathf.MoveTowards(jawAngle, target, jawSpeed * Time.deltaTime);
        ApplyJaws();
    }

    void ApplyJaws()
    {
        // branche droite : +x local, gauche : -x local. Un angle positif les écarte.
        if (branchRight != null) branchRight.localRotation = restRight * Quaternion.AngleAxis(jawAngle, Vector3.up);
        if (branchLeft != null) branchLeft.localRotation = restLeft * Quaternion.AngleAxis(-jawAngle, Vector3.up);
    }

    void LateUpdate()
    {
        // L'objet suit la pointe (sans devenir enfant : le bord de cavité ne doit réagir qu'à la pince)
        if (carried != null && tipPoint != null)
        {
            blend = pinchBlendTime <= 0f ? 1f : Mathf.Min(1f, blend + Time.deltaTime / pinchBlendTime);
            Vector3 local = Vector3.Lerp(startLocalPos, targetLocalPos, blend);
            carried.transform.SetPositionAndRotation(
                tipPoint.TransformPoint(local),
                tipPoint.rotation * carriedLocalRot);
        }
    }

    public void SetClosed(bool value)
    {
        if (closed == value) return;
        closed = value;
        if (closed) { if (tool != null) tool.Vibrate(0.25f, 0.08f); TryPickUp(); }
        else Drop();
    }

    [ContextMenu("Test : fermer la pince")]
    void TestClose() { SetClosed(true); }

    [ContextMenu("Test : ouvrir la pince")]
    void TestOpen() { SetClosed(false); }

    void TryPickUp()
    {
        if (carried != null || tipPoint == null) return;

        BodyPart best = null;
        Transform bestHandle = null;
        float bestDist = grabDistance;
        foreach (Collider col in Physics.OverlapSphere(tipPoint.position, searchRadius))
        {
            BodyPart part = col.GetComponentInParent<BodyPart>();
            if (part == null || part.IsCarried) continue;
            Transform handle = FindHandle(part.transform);
            // on vise la poignée si elle existe, sinon le point de l'objet le plus proche
            float d = handle != null
                ? Vector3.Distance(handle.position, tipPoint.position)
                : Vector3.Distance(col.ClosestPoint(tipPoint.position), tipPoint.position);
            if (d < bestDist) { bestDist = d; best = part; bestHandle = handle; }
        }
        if (best == null) return;

        carried = best;
        carried.PickUp();
        startLocalPos = tipPoint.InverseTransformPoint(best.transform.position);
        // la poignée glisse jusqu'au fond des mâchoires (pointe de la pince)
        Vector3 handleLocal = bestHandle != null ? tipPoint.InverseTransformPoint(bestHandle.position) : startLocalPos;
        targetLocalPos = bestHandle != null ? startLocalPos - handleLocal : startLocalPos;
        carriedLocalRot = Quaternion.Inverse(tipPoint.rotation) * best.transform.rotation;
        blend = 0f;

        // angle où les mâchoires touchent la poignée
        float half = Mathf.Max(0f, handleThickness * 0.5f - jawInnerOffset);
        pinchedAngle = Mathf.Asin(Mathf.Clamp01(half / jawLength)) * Mathf.Rad2Deg;

        if (tool != null) tool.Vibrate(0.6f, 0.15f);
    }

    void Drop()
    {
        if (carried == null) return;
        carried.Release();
        carried = null;
    }

    static Transform FindHandle(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name.StartsWith("Handle_")) return t;
        return null;
    }
}
