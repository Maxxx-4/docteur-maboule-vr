using UnityEngine;

/// <summary>
/// Bord métallique d'une cavité. Si la pince le touche : buzzer, nez rouge, vibration, erreur.
/// Le Collider doit avoir "Is Trigger" coché.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CavityEdge : MonoBehaviour
{
    [SerializeField] AudioSource buzzer;
    [SerializeField] Renderer noseRenderer;
    [SerializeField] Color alertColor = Color.red;
    [SerializeField] float cooldownSeconds = 0.8f;

    float lastHitTime = -999f;
    Color noseBaseColor;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (noseRenderer != null) noseBaseColor = noseRenderer.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        var tool = other.GetComponentInParent<OperationTool>();
        if (tool == null) return;
        if (Time.time - lastHitTime < cooldownSeconds) return;
        lastHitTime = Time.time;

        if (buzzer != null) buzzer.Play();
        tool.Vibrate();
        if (noseRenderer != null)
        {
            noseRenderer.material.color = alertColor;
            CancelInvoke(nameof(ResetNose));
            Invoke(nameof(ResetNose), 0.5f);
        }
        if (GameManager.Instance != null) GameManager.Instance.RegisterError();
    }

    void ResetNose()
    {
        if (noseRenderer != null) noseRenderer.material.color = noseBaseColor;
    }
}
