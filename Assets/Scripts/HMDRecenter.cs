using UnityEngine;
using UnityEngine.XR;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// HMD Recenter: Verschiebt das XR Origin so, dass die Kamera (HMD)
/// genau an der recenterTargetPosition landet — analog zur Teleportlogik
/// die den Camera-Offset vom Origin abzieht.
///
/// Setup:
///   - Script auf das XR Origin GameObject legen.
///   - xrCamera: Camera unter dem Camera Offset zuweisen.
///   - recenterTargetPosition: Transform an dem der Spieler nach dem Recenter steht.
///     Wenn leer → nur Yaw-Rotation wird korrigiert, Position bleibt.
/// </summary>
public class HMDRecenter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Die XR Camera (Kind des Camera Offset im XR Origin).")]
    public Transform xrCamera;

    [Header("Recenter Target (optional)")]
    [Tooltip("Weltposition, auf die die KAMERA (nicht das Origin) gesetzt wird. " +
             "Leer lassen = nur Rotation korrigieren.")]
    public Transform recenterTargetPosition;

    [Header("Optionen")]
    [Tooltip("Yaw des HMDs als neue Vorwärtsrichtung setzen.")]
    public bool recenterRotation = true;

    [Header("Input")]
    public bool useMenuButton = true;
    public KeyCode editorKey = KeyCode.R;

    // ── Private ───────────────────────────────────────────────────────────────

    private bool _menuWasPressed;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (xrCamera == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) xrCamera = cam.transform;
            else Debug.LogError("[HMDRecenter] Keine Camera gefunden — xrCamera manuell zuweisen.");
        }
    }

    private void Update()
    {
        if (ShouldRecenter())
            PerformRecenter();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private bool ShouldRecenter()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current[Key.R].wasPressedThisFrame)
            return true;
#else
        if (Input.GetKeyDown(editorKey)) return true;
#endif

        if (useMenuButton)
        {
            var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            left.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool pressed);
            bool risingEdge = pressed && !_menuWasPressed;
            _menuWasPressed = pressed;
            if (risingEdge) return true;
        }

        return false;
    }

    // ── Core ──────────────────────────────────────────────────────────────────

    public void PerformRecenter()
    {
        if (xrCamera == null) return;

        // 1. Rotation: Origin so drehen, dass HMD-Yaw = neue Vorwärtsrichtung.
        //    Drehen um HMD-Position als Pivot, damit keine Positionsverschiebung entsteht.
        if (recenterRotation)
        {
            float hmdYaw    = xrCamera.eulerAngles.y;
            float originYaw = transform.eulerAngles.y;
            transform.RotateAround(xrCamera.position, Vector3.up, hmdYaw - originYaw);
        }

        // 2. Position: Camera-Offset vom Origin abziehen, sodass die KAMERA
        //    an der Zielposition landet (nicht das Origin selbst).
        //
        //    Formel (analog zu deinem TeleportToEntrance):
        //      cameraOffset = kameraPosition - originPosition  (nur X/Z, Y bleibt)
        //      neueOriginPosition = zielPosition - cameraOffset
        //
        if (recenterTargetPosition != null)
        {
            Vector3 cameraOffset = xrCamera.position - transform.position;
            cameraOffset.y = 0f; // Y nicht anfassen → Floor-Offset bleibt korrekt

            transform.position = recenterTargetPosition.position - cameraOffset;
        }

        Debug.Log($"[HMDRecenter] Recentered. Origin: {transform.position}, " +
                  $"Camera: {xrCamera.position}, Yaw: {transform.eulerAngles.y:F1}°");
    }
}
