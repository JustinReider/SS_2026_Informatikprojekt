using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.SpatialTracking;

public class XROriginSwitcher
{
		public string xrOriginTag;
    private XROrigin xrOrigin;

    public void FindAndDisableXROrigin()
    {
        var allOrigins = Object.FindObjectsByType<XROrigin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        xrOrigin = null;
				if(xrOriginTag!=null)
        foreach (var origin in allOrigins)
        {
            if (origin.CompareTag(xrOriginTag))
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

    public void RestoreXROrigin()
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
