using UnityEngine;

public class MausUmschauen : MonoBehaviour
{
    public Transform sklave_body; // ← Der Sklave Body (Parent)
    public float mouseSensitivity = 2f;
    private float rotationX = 0f;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Vertikal (Kamera oben/unten)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Horizontal (Body links/rechts)
        sklave_body.Rotate(Vector3.up * mouseX);
    }
}
