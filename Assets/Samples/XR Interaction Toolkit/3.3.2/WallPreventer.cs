using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(XROrigin))]
public class WallPreventer : MonoBehaviour
{
    [Header("Einstellungen")]
    [SerializeField] private float minHeight = 0.6f;        // Wichtig! Nicht zu klein machen
    [SerializeField] private float heightOffset = 0.15f;    // Etwas kleiner als vorher
    [SerializeField] private float smoothing = 8f;          // Sanfter Übergang

    [Header("Geschwindigkeit basierend auf Grösse")]
    [SerializeField] private ContinuousMoveProvider moveProvider; // Wird automatisch gesucht, falls leer
    [SerializeField] private float minSpeed = 0.75f;         // Geschwindigkeit, wenn der Spieler ganz geduckt ist
    [SerializeField] private float minSpeedHeight = 0.6f;    // Grösse, bei der minSpeed erreicht wird
    [SerializeField] private float maxSpeed = 2.5f;          // Geschwindigkeit bei normaler/voller Grösse
    [SerializeField] private float maxSpeedHeight = 1.8f;    // Grösse, bei der maxSpeed erreicht wird

    private CharacterController characterController;
    private XROrigin xrOrigin;
    private Transform headTransform;

    private float targetHeight;
    private float currentHeight;

    void Awake()
    {
        if (!TryGetComponent(out characterController) || !TryGetComponent(out xrOrigin))
        {
            Debug.LogWarning("WallPreventer: Fehlende Komponenten. Script wird deaktiviert.");
            enabled = false;
            return;
        }

        headTransform = xrOrigin.Camera.transform;

        if (moveProvider == null)
            moveProvider = GetComponentInChildren<ContinuousMoveProvider>(true);

        if (moveProvider == null)
            Debug.LogWarning("WallPreventer: Kein ContinuousMoveProvider (oder DynamicMoveProvider) gefunden. Geschwindigkeits-Anpassung wird übersprungen.");

        if (minSpeedHeight >= maxSpeedHeight)
            Debug.LogWarning("WallPreventer: minSpeedHeight sollte kleiner als maxSpeedHeight sein.");

        // Initiale Werte setzen
        currentHeight = characterController.height;
        targetHeight = currentHeight;
    }

    void Update()
    {
        if (headTransform == null) return;

        // X/Z Center immer der Kamera folgen lassen
        Vector3 center = characterController.center;
        center.x = headTransform.localPosition.x;
        center.z = headTransform.localPosition.z;

        // Height basierend auf Kopfhöhe berechnen
        float headHeight = headTransform.localPosition.y;
        targetHeight = Mathf.Max(minHeight, headHeight + heightOffset);

        // Sanft interpolieren (verhindert abrupte Änderungen)
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, smoothing * Time.deltaTime);

        // Height und Center aktualisieren
        characterController.height = currentHeight;
        center.y = currentHeight / 2f;
        characterController.center = center;

        // Geschwindigkeit an aktuelle (geglättete) Grösse anpassen
        if (moveProvider != null)
        {
            float heightT = Mathf.InverseLerp(minSpeedHeight, maxSpeedHeight, currentHeight);
            moveProvider.moveSpeed = Mathf.SmoothStep(minSpeed, maxSpeed, heightT);
        }

        // Nur SimpleMove aufrufen wenn wirklich nötig (Performance + Stabilität)
        if (characterController.enabled)
        {
            characterController.SimpleMove(Vector3.zero);
        }
    }

    // Optional: Methode zum manuellen Reset
    public void ResetToDefault()
    {
        if (characterController == null) return;
        
        characterController.height = 1.8f; // Standard Größe
        characterController.center = new Vector3(0, 0.9f, 0);
        currentHeight = 1.8f;
        targetHeight = 1.8f;
    }
}
