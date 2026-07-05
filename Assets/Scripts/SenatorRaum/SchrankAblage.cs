using UnityEngine;

// Trigger-Volumen im/am Schrank-Prefab (Schrank-Rollen), das eine hineingelegte
// Schriftrolle "ablegt" und später wieder entnehmbar macht. Bewusst trigger-basiert
// statt XRSocketInteractor, weil das Projekt bisher keine Sockets verwendet und dieses
// Muster zum bestehenden trigger-basierten Stil passt (siehe SklaveVoiceTrigger.cs).
[RequireComponent(typeof(Collider))]
public class SchrankAblage : MonoBehaviour
{
    [Tooltip("Tag, den das Schriftrollen-Prefab tragen muss.")]
    public string schriftrolleTag = "Schriftrolle";

    [Tooltip("Punkt im Schrank, an dem eine abgelegte Schriftrolle sichtbar liegt.")]
    public Transform ablagePunkt;

    private GameObject abgelegteRolle;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(schriftrolleTag)) return;
        if (abgelegteRolle != null) return; // Schrank schon belegt

        abgelegteRolle = other.gameObject;

        var grab = abgelegteRolle.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null) grab.enabled = false;

        var rb = abgelegteRolle.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (ablagePunkt != null)
        {
            abgelegteRolle.transform.SetPositionAndRotation(ablagePunkt.position, ablagePunkt.rotation);
            abgelegteRolle.transform.SetParent(ablagePunkt, true);
        }
    }

    public GameObject EntnehmenSchriftrolle(Transform entnahmePunkt)
    {
        if (abgelegteRolle == null) return null;

        GameObject rolle = abgelegteRolle;
        abgelegteRolle = null;

        rolle.transform.SetParent(null);
        rolle.transform.SetPositionAndRotation(entnahmePunkt.position, entnahmePunkt.rotation);

        var grab = rolle.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null) grab.enabled = true;

        var rb = rolle.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        return rolle;
    }
}
