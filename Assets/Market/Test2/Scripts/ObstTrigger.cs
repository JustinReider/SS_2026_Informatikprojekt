using UnityEngine;

public class ObstTrigger : MonoBehaviour
{
    public ObstVendor obstVendor;
    public ObstInteraction obstInteraction;
    private bool spielerInZone = false;

    void Update()
    {
        if (spielerInZone && Input.GetKeyDown(KeyCode.F))
        {
            // Prüfe: Begrüßung oder Bezahlung?
            if (!obstInteraction.obstBegruest)
            {
                // Erste F: Begrüßen
                obstVendor.Begruessen();
                obstInteraction.obstBegruest = true;
            }
            else if (obstInteraction.obstCounter > 0)
            {
                // Zweite F: Bezahlen
                obstInteraction.BezahleObst();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            spielerInZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            spielerInZone = false;
    }
}
