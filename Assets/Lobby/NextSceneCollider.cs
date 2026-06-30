using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneCollider : MonoBehaviour
{
    [Header("Szene wechseln")]
    public string sceneNameToLoad = "DeineSzeneName";

	private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("MainCamera"))
        {
            return;
        }
		LoadingScreenManager.Instance.SimpleLoadScene(sceneNameToLoad);
	}
}
