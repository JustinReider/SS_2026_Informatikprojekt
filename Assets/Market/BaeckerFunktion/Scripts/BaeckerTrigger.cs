using UnityEngine;

public class BaeckerTrigger : MonoBehaviour
{
    public BaeckerNPC baecker;
    private bool spielerInZone = false;
    public BrotInteraction brotInteraction;

    void Update()
    {
        if (spielerInZone && Input.GetKeyDown(KeyCode.F))
        {
            baecker.Begruessen();
	    brotInteraction.baeckerBegruest = true;
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
