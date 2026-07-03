using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class BaeckerNPC : MonoBehaviour
{
    public Animator animator;
    private bool bereitsBegruest = false;
    public Transform regalPosition;
    public Transform spielerPosition;
    public GameObject brotPrefab;
    public Transform brotSpawnPunkt;
    public Transform brotVorratParent;
    public bool IstBrotAufTisch = false;

    public AudioClip clipWillkommen;
    public AudioClip clipAllesKlar;
    public AudioClip clipPreis;
    public AudioClip clipDanke;
    private AudioSource audioSource;

    private enum Zustand { Idle, DrehtZumRegal, DrehtZumSpieler }
    private Zustand zustand = Zustand.Idle;

    private XRSimpleInteractable interactable;
    private Transform aktuellesBrotAufTresen;
    private const float BrotVerlorenAbstand = 0.5f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.activated.AddListener(OnActivated);
    }

    void OnDestroy()
    {
        interactable.activated.RemoveListener(OnActivated);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        Begruessen();
        HoleBrot();
    }

    public void Begruessen()
    {
        if (bereitsBegruest) return;
        bereitsBegruest = true;
        audioSource.PlayOneShot(clipWillkommen);
        Debug.Log("Bäcker: Salve! Quid vis?");
    }

    public void HoleBrot()
    {
        if (IstBrotAufTisch || zustand != Zustand.Idle) return;
        if (NaechstesVorratItem() == null)
        {
            Debug.Log("Bäcker: Kein Brot mehr im Regal!");
            return;
        }

        zustand = Zustand.DrehtZumRegal;
        audioSource.PlayOneShot(clipAllesKlar);
        Invoke("DreheZurueck", 2f);
    }

    private GameObject NaechstesVorratItem()
    {
        if (brotVorratParent == null) return null;

        foreach (Transform brot in brotVorratParent)
        {
            if (brot.gameObject.activeSelf)
                return brot.gameObject;
        }
        return null;
    }

    public void SpieleDanke()
    {
        audioSource.PlayOneShot(clipDanke);
    }

    void DreheZurueck()
    {
        zustand = Zustand.DrehtZumSpieler;
        Invoke("BrotAufTischLegen", 2f);
    }

    void BrotAufTischLegen()
    {
        zustand = Zustand.Idle;

        GameObject naechstesBrot = NaechstesVorratItem();
        if (naechstesBrot != null)
        {
            naechstesBrot.tag = "Untagged";
            naechstesBrot.SetActive(false);
        }

        if (brotPrefab != null && brotSpawnPunkt != null)
        {
            GameObject brotInstanz = Instantiate(brotPrefab, brotSpawnPunkt.position, Quaternion.Euler(270f, 0f, 90f));
            aktuellesBrotAufTresen = brotInstanz.transform;

            XRGrabInteractable grab = brotInstanz.GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.selectEntered.AddListener(_ => IstBrotAufTisch = false);
        }

        IstBrotAufTisch = true;
        audioSource.PlayOneShot(clipPreis);
        Debug.Log("Brot liegt auf dem Tresen!");
    }

    public void ResetBegruessung()
    {
	bereitsBegruest = false;
    }

    void Update()
    {
        if (zustand == Zustand.DrehtZumRegal && regalPosition != null)
        {
            DreheZu(regalPosition.position);
        }
        else if (zustand == Zustand.DrehtZumSpieler)
        {
            Transform ziel = spielerPosition != null ? spielerPosition : (Camera.main != null ? Camera.main.transform : null);
            if (ziel != null)
                DreheZu(ziel.position);
        }

        // Falls das Brot vom Tresen gestoßen wurde und nie aufgehoben wird, blockiert
        // IstBrotAufTisch sonst für immer den nächsten Kauf.
        if (IstBrotAufTisch && aktuellesBrotAufTresen != null && brotSpawnPunkt != null)
        {
            float abstand = Vector3.Distance(aktuellesBrotAufTresen.position, brotSpawnPunkt.position);
            if (abstand > BrotVerlorenAbstand)
                IstBrotAufTisch = false;
        }
    }

    private void DreheZu(Vector3 zielPosition)
    {
        Vector3 richtung = zielPosition - transform.position;
        richtung.y = 0;
        if (richtung.sqrMagnitude < 0.0001f) return;
        Quaternion ziel = Quaternion.LookRotation(richtung);
        transform.rotation = Quaternion.Slerp(transform.rotation, ziel, Time.deltaTime * 3f);
    }
}
