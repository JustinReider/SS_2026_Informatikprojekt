using UnityEngine;

public class SklavenFollowBauer : MonoBehaviour
{
    public Transform bauer;
    public float abstand = 1f; // 1 Meter hinter Bauern
    public float speed = 2f; // Wie schnell folgen?

    void Update()
    {
        // Zielposition: 1m hinter Bauern
        Vector3 zielPosition = bauer.position - bauer.forward * abstand;
        zielPosition.y = transform.position.y; // Höhe beibehalten

        // Sanft dahin bewegen
        transform.position = Vector3.Lerp(transform.position, zielPosition, speed * Time.deltaTime);

        // Blickrichtung: Bauern folgen (leicht nach oben)
    }
}
