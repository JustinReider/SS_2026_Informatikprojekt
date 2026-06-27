using Unity.XR.CoreUtils;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(XROrigin))]
public class WallPreventer : MonoBehaviour
{
    [Header("Einstellungen")]
    [SerializeField] private float minHeight = 0.6f;        // Wichtig! Nicht zu klein machen
    [SerializeField] private float heightOffset = 0.15f;    // Etwas kleiner als vorher
    [SerializeField] private float smoothing = 8f;          // Sanfter Übergang

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
