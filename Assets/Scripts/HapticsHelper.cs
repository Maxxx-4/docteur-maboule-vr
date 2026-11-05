using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Envoie une vibration à une manette. Sans effet (et sans erreur) dans le simulateur XR
/// ou si la manette ne gère pas le haptique.
/// </summary>
public static class HapticsHelper
{
    public static void Pulse(XRNode node, float amplitude = 0.7f, float duration = 0.2f)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid) return;

        if (device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
        {
            device.SendHapticImpulse(0, Mathf.Clamp01(amplitude), duration);
        }
    }
}
