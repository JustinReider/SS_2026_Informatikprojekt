using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.SpatialTracking;

public class XROriginSwitcher: MonoBehaviour
{
    [Header("Einstellungen")]
    private XROrigin xrOrigin;

    void Awake()
    {
    }

    void Start()
    {
        FindAndDisableXROrigin();
    }

    private void FindAndDisableXROrigin()
		{
		    // Alle XROrigins holen (inkl. inaktive), dann nach "Player"-Tag filtern
		    var allOrigins = FindObjectsByType<XROrigin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		    
		    xrOrigin = null;
		    foreach (var origin in allOrigins)
		    {
		        if (origin.CompareTag("Player"))
		        {
		            xrOrigin = origin;
		            break;
		        }
		    }
		
		    if (xrOrigin == null)
		    {
		        Debug.LogWarning("Kein XROrigin mit Tag 'Player' gefunden.");
		        return;
		    }
		
		    xrOrigin.enabled = false;
				xrOrigin.gameObject.SetActive(false);
		    if (xrOrigin.Camera != null)
		    {
		        xrOrigin.Camera.enabled = false;
		        xrOrigin.Camera.gameObject.SetActive(false);
		    }
		    var oldDrivers = xrOrigin.GetComponentsInChildren<TrackedPoseDriver>(true);
		    foreach (var d in oldDrivers)
		        d.enabled = false;
		
		    Debug.Log("✅ XROrigin deaktiviert: " + xrOrigin.name);
		}

		private void OnDestroy()
		{
		    if (xrOrigin == null) return;
		
		    xrOrigin.enabled = true;
				xrOrigin.gameObject.SetActive(true);
		
		    if (xrOrigin.Camera != null)
		    {
		        xrOrigin.Camera.enabled = true;
		        xrOrigin.Camera.gameObject.SetActive(true);
		    }
		
		    var drivers = xrOrigin.GetComponentsInChildren<TrackedPoseDriver>(true);
		    foreach (var d in drivers)
		        d.enabled = true;
		
		    Debug.Log("✅ XROrigin wiederhergestellt: " + xrOrigin.name);
		}
}
