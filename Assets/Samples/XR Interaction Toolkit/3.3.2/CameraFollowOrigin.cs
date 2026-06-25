using Unity.XR.CoreUtils;
using UnityEngine;

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(XROrigin))]
public class CameraFollowOrigin : MonoBehaviour
{
    CharacterController m_CharacterController;
    Transform m_CameraTransform;
    XROrigin m_XROrigin;

    void Awake()
    {
        TryGetComponent(out m_CharacterController);
        TryGetComponent(out m_XROrigin);
        m_CameraTransform = m_XROrigin.Camera.transform;
    }

    void Update()
    {
        // Wo ist die Camera in Weltkoordinaten (nur X/Z, Y vom Character Controller)
        Vector3 cameraWorldPos = m_CameraTransform.position;

        // Origin zur Camera verschieben (X/Z), Y bleibt vom CC kontrolliert
        transform.position = new Vector3(
            cameraWorldPos.x,
            transform.position.y,
            cameraWorldPos.z
        );

        // Camera Offset wieder auf 0 setzen damit kein Drift entsteht
        m_XROrigin.CameraFloorOffsetObject.transform.localPosition = Vector3.zero;

        // CC center auf 0 halten (Origin ist jetzt direkt unter Camera)
        m_CharacterController.center = new Vector3(
            0,
            m_CharacterController.center.y,
            0
        );

        // Gravity + Physics update
        if (!m_CharacterController.isGrounded)
            m_CharacterController.Move(Physics.gravity * Time.deltaTime);
        else
            m_CharacterController.SimpleMove(Vector3.zero);
    }
}
