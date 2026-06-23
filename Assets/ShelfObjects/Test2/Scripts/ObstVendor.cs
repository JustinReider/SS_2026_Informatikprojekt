using UnityEngine;

public class ObstVendor : MonoBehaviour
{
    public Animator animator;
    private bool bereitsBegruest = false;
    public Transform regalPosition;
    public Transform spielerPosition;
    public GameObject obstPrefab;
    public Transform obstSpawnPunkt;
    public GameObject obstImRegal;
    public bool IstObstAufTisch = false;

    public AudioClip clipWillkommen;
    public AudioClip clipAllesKlar;
    public AudioClip clipPreis;
    public AudioClip clipVultisne;
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
        Debug.Log("Obst Vendor: Salve!");
    }

    public void HoleObst()
    {
        if (IstObstAufTisch || zustand != Zustand.Idle) return;
        zustand = Zustand.DrehtZumRegal;
        audioSource.PlayOneShot(clipAllesKlar);
        Invoke("DreheZurueck", 2f);
    }

    public void SpieleDanke()
    {
        audioSource.PlayOneShot(clipDanke);
    }

    public void ResetBegruessung()
    {
        bereitsBegruest = false;
    }

    void DreheZurueck()
    {
        zustand = Zustand.DrehtZumSpieler;
        Invoke("ObstAufTischLegen", 2f);
    }

    void ObstAufTischLegen()
    {
        zustand = Zustand.Idle;

        if (obstImRegal != null)
        {
            obstImRegal.tag = "Untagged";
            obstImRegal.SetActive(false);
        }

        if (obstPrefab != null && obstSpawnPunkt != null)
            Instantiate(obstPrefab, obstSpawnPunkt.position, Quaternion.Euler(90f, 0f, 180f));

        IstObstAufTisch = true;
        audioSource.PlayOneShot(clipVultisne);
        Debug.Log("Obst liegt auf dem Tresen!");
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
