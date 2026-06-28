using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

/// <summary>
/// HMD Position Reset / Recenter für Unity XR Rig (XRI 2.x / 3.x)
/// 
/// Zentriert den Spieler so, dass:
///   - die Kamera an der definierten Zielposition landet
///   - die Blickrichtung (Yaw) des HMDs als neue Vorwärts-Richtung übernommen wird
///   - die Y-Achse (Höhe) unverändert bleibt
///
/// Setup:
///   1. Script auf ein beliebiges GameObject ziehen (z.B. XR Origin selbst)
///   2. xrOrigin-Referenz im Inspector setzen (oder wird per FindFirstObjectByType gefunden)
///   3. recenterTarget = Transform, an dem der Spieler nach dem Reset stehen soll
///      (leer lassen = Reset an aktueller XZ-Position, nur Rotation wird korrigiert)
///   4. Trigger: RecenterHMD() per Code aufrufen, oder InputAction im Inspector binden
///
/// Auslösen per Controller-Button:
///   Entweder direkt per InputAction (recenterAction im Inspector befüllen)
///   oder RecenterHMD() per UnityEvent / anderen Script-Aufruf triggern.
/// </summary>
public class HMDRecenter : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("Das XROrigin-GameObject. Wird automatisch gesucht wenn leer.")]
    public XROrigin xrOrigin;

    [Tooltip("Optional: Zielpunkt, wo der Spieler nach dem Reset stehen soll (XZ). " +
             "Leer = aktuelle XZ-Position beibehalten, nur Yaw wird korrigiert.")]
    public Transform recenterTarget;

    [Header("Einstellungen")]
    [Tooltip("Yaw (Horizontalrotation) der Kamera als neue Vorwärts-Richtung übernehmen.")]
    public bool matchCameraForward = true;

    [Tooltip("Spieler auf XZ-Position des recenterTarget teleportieren (nur wenn Target gesetzt).")]
    public bool matchTargetPosition = true;

    [Header("Input (optional)")]
    [Tooltip("InputAction für den Recenter-Button. Kann auch per Code aufgerufen werden.")]
    public UnityEngine.InputSystem.InputAction recenterAction;

    // -----------------------------------------------------------------------

    private void Awake()
    {
        if (xrOrigin == null)
            xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null)
            Debug.LogError("[HMDRecenter] Kein XROrigin gefunden! Bitte im Inspector setzen.");
    }

    private void OnEnable()
    {
        if (recenterAction != null)
        {
            recenterAction.performed += _ => RecenterHMD();
            recenterAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (recenterAction != null)
        {
            recenterAction.performed -= _ => RecenterHMD();
            recenterAction.Disable();
        }
    }

    // -----------------------------------------------------------------------
    // Öffentliche Methode — kann von beliebigem anderen Script aufgerufen werden,
    // z.B. aus eurem Voice2Action-System oder einem UnityEvent-Button
    // -----------------------------------------------------------------------
    public void RecenterHMD()
    {
        if (xrOrigin == null)
        {
            Debug.LogWarning("[HMDRecenter] Kein XROrigin — Recenter abgebrochen.");
            return;
        }

        Camera hmdCamera = xrOrigin.Camera;
        if (hmdCamera == null)
        {
            Debug.LogWarning("[HMDRecenter] XROrigin.Camera ist null.");
            return;
        }

        // --- Schritt 1: Position zentrieren ---
        // Kamera soll an Zielposition landen (XZ), Y kommt vom Tracking selbst.
        if (matchTargetPosition && recenterTarget != null)
        {
            // Zielposition: XZ vom Target, Y bleibt wie aktuell (Floor-Tracking)
            Vector3 targetPos = new Vector3(
                recenterTarget.position.x,
                hmdCamera.transform.position.y,   // Y nicht anfassen
                recenterTarget.position.z
            );
            xrOrigin.MoveCameraToWorldLocation(targetPos);
        }

        // --- Schritt 2: Yaw-Rotation korrigieren ---
        // Nur horizontale Vorwärtsrichtung der Kamera übernehmen (kein Pitch/Roll)
        if (matchCameraForward)
        {
            Vector3 cameraForwardFlat = hmdCamera.transform.forward;
            cameraForwardFlat.y = 0f;

            if (cameraForwardFlat.sqrMagnitude > 0.001f)
            {
                cameraForwardFlat.Normalize();

                // MatchOriginUpCameraForward dreht das XROrigin so, dass
                // die Kamera in cameraForwardFlat-Richtung schaut
                xrOrigin.MatchOriginUpCameraForward(Vector3.up, cameraForwardFlat);
            }
        }

        Debug.Log("[HMDRecenter] Recentered.");
    }

    // -----------------------------------------------------------------------
    // Convenience: auch per Tastatur testbar im Editor
    // -----------------------------------------------------------------------
#if UNITY_EDITOR
    [Header("Editor-Test")]
    [Tooltip("Tastenkürzel zum Testen im Play-Mode (nur Editor)")]
    public KeyCode editorTestKey = KeyCode.R;

    private void Update()
    {
        if (Input.GetKeyDown(editorTestKey))
            RecenterHMD();
    }
#endif
}
