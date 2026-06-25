using Unity.XR.CoreUtils;
using UnityEngine;

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(XROrigin))]
public class WallPreventer : MonoBehaviour
{
    CharacterController m_CharacterController;
    Transform m_LocalHeadTransform;
		[SerializeField] private float heightOffset = 0.2f;

    void Awake()
    {  
        if(!TryGetComponent(out m_CharacterController) || !TryGetComponent(out XROrigin xrOrigin))
        {
            Debug.LogWarning("Missing Components. Disabling Now.");
            this.enabled = false;
            return;
        }
        m_LocalHeadTransform = xrOrigin.Camera.transform;
    }

    void Update()
		{
		    // X/Z Center nachziehen
		    m_CharacterController.center = new Vector3(
		        m_LocalHeadTransform.localPosition.x,
		        m_CharacterController.center.y,
		        m_LocalHeadTransform.localPosition.z
		    );
		
		    // Height dynamisch an Kopfhöhe anpassen
		    
				float headHeight = m_LocalHeadTransform.localPosition.y;
				m_CharacterController.height = Mathf.Max(0.3f, headHeight + heightOffset);
				m_CharacterController.center = new Vector3(
				    m_CharacterController.center.x,
				    (headHeight + heightOffset) / 2f,
				    m_CharacterController.center.z
				);
		
		    m_CharacterController.SimpleMove(Vector3.zero);
		}
}
