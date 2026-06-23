using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathCutsceneTrigger : MonoBehaviour
{
    public string SceneName = "";
		public void OpenScene() {
			LoadingScreenManager.Instance.SimpleLoadScene(SceneName);
		}
}
