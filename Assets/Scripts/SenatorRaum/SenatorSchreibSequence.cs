using UnityEngine;

// Orchestriert den gesamten Ablauf der Senator-Schreibszene: Hinlegen -> Sklave kommt
// -> Diktat -> Aufstehen -> Text lesen -> Schriftrolle erhalten. Zustand als einfaches
// privates Enum, analog zu BaeckerNPC.Zustand.
public class SenatorSchreibSequence : MonoBehaviour
{
    private enum Zustand { Bereit, SklaveKommt, Diktiert, TextWirdGelesen }
    private Zustand zustand = Zustand.Bereit;

    [Header("Referenzen")]
    public LiegeInteractable liege;
    public SklaveSchreiber sklave;
    public DiktatController diktat;
    public DiktatPapier papier;

    [Header("Schriftrolle")]
    public GameObject schriftrollePrefab;
    public Transform schriftrolleSpawnPunkt;

    void Start()
    {
        liege.OnHingelegt += SpielerHatSichHingelegt;
        diktat.OnZwischenstand += papier.ZeigeZwischenstand;
        diktat.OnGesamtTextGeaendert += papier.ZeigeZwischenstand;
        diktat.OnDiktatBeendet += DiktatWurdeBeendet;
    }

    void OnDestroy()
    {
        liege.OnHingelegt -= SpielerHatSichHingelegt;
        diktat.OnZwischenstand -= papier.ZeigeZwischenstand;
        diktat.OnGesamtTextGeaendert -= papier.ZeigeZwischenstand;
        diktat.OnDiktatBeendet -= DiktatWurdeBeendet;
    }

    private void SpielerHatSichHingelegt()
    {
        if (zustand != Zustand.Bereit) return;
        zustand = Zustand.SklaveKommt;
        sklave.GeheZuTischUndSetzeDich(SklaveIstBereit);
    }

    private void SklaveIstBereit()
    {
        zustand = Zustand.Diktiert;
        diktat.StartDiktat();
    }

    private void DiktatWurdeBeendet(string vollstaendigerText)
    {
        sklave.SteheAufUndGeheRaus();
        liege.SpielerStehtAuf();
        papier.ZeigeEndgueltig(vollstaendigerText);
        zustand = Zustand.TextWirdGelesen;
    }

    // Wird von PapierSchliessenButton aufgerufen.
    public void PapierSchliessenUndSchriftrolleErhalten()
    {
        if (zustand != Zustand.TextWirdGelesen) return;

        string text = diktat.GetGesamtText();
        papier.Verstecken();

        GameObject rolle = Instantiate(schriftrollePrefab, schriftrolleSpawnPunkt.position, schriftrolleSpawnPunkt.rotation);
        rolle.GetComponent<SchriftrolleItem>().SetText(text);

        zustand = Zustand.Bereit;
    }
}
