using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.SpatialTracking;

public class HMDCameraController : MonoBehaviour
{
    [Header("Einstellungen")]
    [SerializeField] private bool resetOffsetOnStart = true;

    private Camera cam;
    private TrackedPoseDriver driver;
    private XROrigin xrOrigin;

    void Awake()
    {
        cam = GetComponent<Camera>();
        
        driver = GetComponent<TrackedPoseDriver>();
        if (driver == null)
            driver = gameObject.AddComponent<TrackedPoseDriver>();
    }

    void Start()
    {
        FindAndDisableXROrigin();
        SetupDriver();

        if (resetOffsetOnStart)
            ResetCameraOffset();
    }

    private void FindAndDisableXROrigin()
    {
        xrOrigin = FindFirstObjectByType<XROrigin>(FindObjectsInactive.Include);
        if (xrOrigin == null)
        {
            Debug.LogWarning("Kein XROrigin gefunden.");
            return;
        }

        // Komplett deaktivieren
        xrOrigin.enabled = false;
        
        if (xrOrigin.Camera != null)
        {
            xrOrigin.Camera.enabled = false;
            xrOrigin.Camera.gameObject.SetActive(false);
        }

        // Zusätzlich alle TrackedPoseDriver auf dem alten Rig deaktivieren
        var oldDrivers = xrOrigin.GetComponentsInChildren<TrackedPoseDriver>(true);
        foreach (var d in oldDrivers)
            d.enabled = false;

        Debug.Log("✅ Altes XROrigin vollständig deaktiviert");
    }

    private void SetupDriver()
    {
        // Legacy SpatialTracking Driver
        driver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);

        Debug.Log("✅ HMD Tracking auf Center (GenericXRDevice) gesetzt");
    }

    public void ResetCameraOffset()
    {
        StartCoroutine(ResetOffsetRoutine());
    }

    private System.Collections.IEnumerator ResetOffsetRoutine()
    {
        yield return null;
        yield return null;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log("✅ Kamera-Offset zurückgesetzt");
    }
}
