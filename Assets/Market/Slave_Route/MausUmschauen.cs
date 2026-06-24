using UnityEngine;

public class MausUmschauen : MonoBehaviour
{
    public float mouseSensitivity = 2f;
    private float rotationX = 0f;

    void Update()
    {
        // Maus Input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Vertikal schauen (oben/unten)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Anwenden
        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        // Parent (Sklave Body)
    }
}
