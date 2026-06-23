using UnityEngine;
using TMPro;

public class BrotInteraction : MonoBehaviour
{
    private Camera cam;
    private GameObject letztesObjekt;
    public BaeckerNPC baecker;
    public Geldbeutel geldbeutel;
    public Inventory inventory;
    public bool baeckerBegruest = false;

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
            Debug.Log("Treffer: " + hit.collider.name + " | Tag: " + hit.collider.tag);

            if (hit.collider.CompareTag("Brot"))
            {
                Debug.Log("BROT GEFUNDEN!");
                
                if (letztesObjekt != hit.collider.gameObject)
                {
                    if (letztesObjekt != null)
                        letztesObjekt.GetComponent<Outline>().enabled = false;

                    letztesObjekt = hit.collider.gameObject;
                    letztesObjekt.GetComponent<Outline>().enabled = true;
                    Debug.Log("Outline aktiviert!");
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!baeckerBegruest)
                    {
                        Debug.Log("Erst den Bäcker begrüßen! (F-Taste)");
                        return;
                    }

                    if (!baecker.IstBrotAufTisch)
                    {
                        Debug.Log("Hole Brot...");
                        baecker.HoleBrot();
                    }
                    else
                    {
                        Debug.Log("Versuche zu bezahlen...");
                        if (geldbeutel.BezahleAs(1))
                        {
                            baecker.SpieleDanke();
                            baecker.IstBrotAufTisch = false;
                            baecker.ResetBegruessung();
			    
			    baeckerBegruest =false;

                            inventory.AddItem("Brot");
                            Destroy(letztesObjekt);
                            letztesObjekt = null;
                            
                            Debug.Log("Brot gekauft!");
                        }
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
        else
        {
            if (letztesObjekt != null)
            {
                letztesObjekt.GetComponent<Outline>().enabled = false;
                letztesObjekt = null;
            }
        }
    }
}
