using UnityEngine;

public class SklavenKamera : MonoBehaviour
{
    public Transform bauer;
    public float abstand = 1f; // 1 Meter hinter dem Bauern
    public float hoehe = 1.6f; // Augenhöhe

    void LateUpdate()
    {
        // Position: 1m hinter Bauern
        Vector3 zielPosition = bauer.position - bauer.forward * abstand;
        zielPosition.y = bauer.position.y + hoehe;
        transform.position = zielPosition;

        // Blickrichtung: zum Bauern hin (leicht über Schulter)
        transform.LookAt(bauer.position + Vector3.up * hoehe);
    }
}
