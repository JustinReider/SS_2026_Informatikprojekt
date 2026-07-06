using UnityEngine;
using TMPro;

// Zeigt den diktierten Text als World-Space-Text auf einem Papier/einer Schriftrolle
// an (gleiches Prinzip wie SoldatAnweiser.cs, nur ohne Typewriter-Effekt, weil der
// Text hier schon live durch das Diktat "wächst").
public class DiktatPapier : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("World-Space TextMeshPro, auf der der Text erscheint.")]
    public TextMeshPro papierText;

    [Header("Darstellung")]
    [Tooltip("Farbe für die noch nicht endgültige (gehörte, aber nicht bestätigte) Live-Vorschau.")]
    public Color zwischenstandFarbe = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    public Color endgueltigFarbe = Color.black;

    void Awake()
    {
        if (papierText == null)
            papierText = GetComponentInChildren<TextMeshPro>(true);

        Verstecken();
    }

    public void ZeigeZwischenstand(string vorschauText)
    {
        gameObject.SetActive(true);
        papierText.color = zwischenstandFarbe;
        papierText.text = vorschauText;
    }

    public void ZeigeEndgueltig(string text)
    {
        gameObject.SetActive(true);
        papierText.color = endgueltigFarbe;
        papierText.text = text;
    }

    public void Verstecken()
    {
        gameObject.SetActive(false);
    }
}
