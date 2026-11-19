using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// À placer sur la pince, avec un Collider + Rigidbody.
/// Sert de "marqueur" : CavityEdge détecte ce composant au lieu d'utiliser un tag.
/// Sait aussi faire vibrer la manette qui tient la pince.
/// </summary>
public class OperationTool : MonoBehaviour
{
    [Tooltip("Main qui tient la pince (pour la vibration de la manette).")]
    public XRNode hand = XRNode.RightHand;

    /// <summary>Fait vibrer la manette. Sans effet (et sans erreur) dans le simulateur XR.</summary>
    public void Vibrate(float amplitude = 0.8f, float duration = 0.25f)
    {
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(hand);
        if (device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
            device.SendHapticImpulse(0, Mathf.Clamp01(amplitude), duration);
    }
}
