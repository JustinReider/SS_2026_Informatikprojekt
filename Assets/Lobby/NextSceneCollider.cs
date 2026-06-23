using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneCollider : MonoBehaviour
{
    [Header("Szene wechseln")]
    public string sceneNameToLoad = "DeineSzeneName";

		private void OnTriggerEnter(Collider other) {
    		// Szene laden
				LoadingScreenManager.Instance.SimpleLoadScene(sceneNameToLoad);
		}
}
