using UnityEngine;

public class ObstInteraction : MonoBehaviour
{
    private Camera cam;
    private GameObject letztesObjekt;
    public ObstVendor obstVendor;
    public Geldbeutel geldbeutel;
    public Inventory inventory;
    public bool obstBegruest = false;
    public int obstCounter = 0; // ← Zähler!

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red);

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.CompareTag("Obst"))
            {
                if (letztesObjekt != hit.collider.gameObject)
                {
                    if (letztesObjekt != null)
                        letztesObjekt.GetComponent<Outline>().enabled = false;

                    letztesObjekt = hit.collider.gameObject;
                    letztesObjekt.GetComponent<Outline>().enabled = true;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!obstBegruest)
                    {
                        Debug.Log("Erst den Vendor begrüßen! (F-Taste)");
                        return;
                    }

                    // Apfel hinzufügen!
                    obstCounter++;
                    Debug.Log("Äpfel ausgewählt: " + obstCounter);
                }
            }
            else
            {
                if (letztesObjekt != null)
                {
                    letztesObjekt.GetComponent<Outline>().enabled = false;
                    letztesObjekt = null;
                }
            }
        }
        else
        {
            if (letztesObjekt != null)
            {
                letztesObjekt.GetComponent<Outline>().enabled = false;
                letztesObjekt = null;
            }
        }
    }

    public void BezahleObst()
    {
        int preis = obstCounter * 10; // 10 As pro Apfel
        
        if (geldbeutel.BezahleAs(preis))
        {
            obstVendor.SpieleDanke();
            obstVendor.ResetBegruessung();
            
            for (int i = 0; i < obstCounter; i++)
            {
                inventory.AddItem("Apfel");
            }
            
            Debug.Log(obstCounter + " Äpfel gekauft für " + preis + " As!");
            
            // Reset
            obstBegruest = false;
            obstCounter = 0;
            letztesObjekt = null;
        }
    }
}
