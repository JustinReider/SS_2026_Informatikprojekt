using UnityEngine;
using TMPro;

// Sitzt auf dem Schriftrollen-Prefab. Trägt den diktierten Text und wird von
// XRGrabInteractable (im Prefab selbst konfiguriert, wie beim Bäcker-Brot) gehalten.
public class SchriftrolleItem : MonoBehaviour
{
    [Tooltip("World-Space TextMeshPro auf dem Schriftrollen-Modell, auf der der diktierte Text zu lesen ist.")]
    public TextMeshPro rollenText;

    public string Text { get; private set; } = "";

    public void SetText(string text)
    {
        Text = text;
        if (rollenText != null)
            rollenText.text = text;
    }
}
