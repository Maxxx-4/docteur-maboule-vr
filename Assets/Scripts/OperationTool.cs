using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// À placer sur la pince (tweezers), avec un Collider + Rigidbody.
/// Sert de "marqueur" : CavityEdge détecte ce composant au lieu d'utiliser un tag.
/// </summary>
public class OperationTool : MonoBehaviour
{
    [Tooltip("Main qui tient la pince (pour la vibration de la manette).")]
    public XRNode hand = XRNode.RightHand;

    public void Vibrate(float amplitude = 0.8f, float duration = 0.25f)
    {
        HapticsHelper.Pulse(hand, amplitude, duration);
    }
}
