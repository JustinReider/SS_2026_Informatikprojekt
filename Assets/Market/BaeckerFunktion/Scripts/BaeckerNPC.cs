using UnityEngine;

public class BaeckerNPC : MonoBehaviour
{
    public Animator animator;
    private bool bereitsBegruest = false;
    public Transform regalPosition;
    public Transform spielerPosition;
    public GameObject brotPrefab;
    public Transform brotSpawnPunkt;
    public GameObject brotImRegal;
    public bool IstBrotAufTisch = false;

    public AudioClip clipWillkommen;
    public AudioClip clipAllesKlar;
    public AudioClip clipPreis;
    public AudioClip clipDanke;
    private AudioSource audioSource;

    private enum Zustand { Idle, DrehtZumRegal, DrehtZumSpieler }
    private Zustand zustand = Zustand.Idle;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
        zustand = Zustand.DrehtZumRegal;
        audioSource.PlayOneShot(clipAllesKlar);
        Invoke("DreheZurueck", 2f);
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

        if (brotImRegal != null)
        {
            brotImRegal.tag = "Untagged";
            brotImRegal.SetActive(false);
        }

        if (brotPrefab != null && brotSpawnPunkt != null)
            Instantiate(brotPrefab, brotSpawnPunkt.position, Quaternion.Euler(270f, 0f, 90f));

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
        if (zustand == Zustand.DrehtZumRegal)
        {
            Vector3 richtung = regalPosition.position - transform.position;
            richtung.y = 0;
            Quaternion ziel = Quaternion.LookRotation(richtung);
            transform.rotation = Quaternion.Slerp(transform.rotation, ziel, Time.deltaTime * 3f);
        }
        else if (zustand == Zustand.DrehtZumSpieler)
        {
            Vector3 richtung = spielerPosition.position - transform.position;
            richtung.y = 0;
            Quaternion ziel = Quaternion.LookRotation(richtung);
            transform.rotation = Quaternion.Slerp(transform.rotation, ziel, Time.deltaTime * 3f);
        }
    }
}
