using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ItemAttentionPulse : MonoBehaviour
{
    public enum PulseMode
    {
        NurVorErstemAufheben,
        ImmerWennNichtGehalten
    }

    [Header("Verhalten")]
    [Tooltip("NurVorErstemAufheben: Das Pulsieren erlischt für immer, sobald das Item einmal aufgehoben wurde.\nImmerWennNichtGehalten: Das Pulsieren kehrt nach jedem Loslassen zurück.")]
    public PulseMode pulseMode = PulseMode.NurVorErstemAufheben;

    [Tooltip("Wartezeit nach dem Loslassen, bevor das Pulsieren wieder einsetzt (nur bei ImmerWennNichtGehalten). Verhindert, dass das Item sofort wieder aufleuchtet.")]
    [Min(0f)]
    public float resumeDelay = 4f;

    [Header("Glow-Optik")]
    [Tooltip("Farbe des Leuchteffekts (HDR). Warmes, gedämpftes Licht wirkt in Rom/der Mine am stimmigsten.")]
    [ColorUsage(false, true)]
    public Color glowColor = new Color(1f, 0.72f, 0.35f);

    [Tooltip("Maximale Emissions-Stärke am Höhepunkt des Pulsierens. Niedrig halten, um die Immersion nicht zu stören.")]
    [Range(0f, 3f)]
    public float maxIntensity = 0.8f;

    [Tooltip("Geschwindigkeit des Pulsierens")]
    [Range(0.1f, 5f)]
    public float pulseSpeed = 1.2f;

    [Tooltip("Untergrenze der Pulswelle (0 = Glühen erlischt komplett zwischendurch, >0 = bleibt leicht sichtbar)")]
    [Range(0f, 1f)]
    public float minPulseFraction = 0f;

    [Tooltip("Renderer, deren Emission pulsieren soll. Leer lassen, um automatisch alle Renderer in diesem Objekt und seinen Kindern zu verwenden.")]
    public Renderer[] targetRenderers;

    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    XRGrabInteractable grabInteractable;
    MaterialPropertyBlock propertyBlock;

    bool hasBeenPickedUp;
    bool isHeld;
    bool isPulsing;
    float resumeTimer;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        propertyBlock = new MaterialPropertyBlock();

        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>();

        // Emission-Keyword muss auf den Material-Instanzen aktiv sein, sonst hat die
        // Property-Block-Farbe im URP/Lit-Shader keine Wirkung.
        foreach (var r in targetRenderers)
        {
            foreach (var mat in r.materials)
                mat.EnableKeyword("_EMISSION");
        }

        isPulsing = true;
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        grabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        isHeld = true;
        hasBeenPickedUp = true;
        isPulsing = false;
        SetEmission(Color.black);
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        isHeld = false;

        if (pulseMode == PulseMode.NurVorErstemAufheben)
        {
            // Für diesen Modus ist das Pulsieren nach dem ersten Aufheben endgültig vorbei.
            isPulsing = false;
            SetEmission(Color.black);
            return;
        }

        resumeTimer = resumeDelay;
    }

    void Update()
    {
        if (isHeld) return;

        if (!isPulsing)
        {
            if (pulseMode != PulseMode.ImmerWennNichtGehalten || !hasBeenPickedUp)
                return;

            resumeTimer -= Time.deltaTime;
            if (resumeTimer > 0f) return;

            isPulsing = true;
        }

        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // sanfte 0..1 Welle
        float t = Mathf.Lerp(minPulseFraction, 1f, wave);
        SetEmission(glowColor * (t * maxIntensity));
    }

    void SetEmission(Color color)
    {
        foreach (var r in targetRenderers)
        {
            r.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorId, color);
            r.SetPropertyBlock(propertyBlock);
        }
    }
}
