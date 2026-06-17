using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour {

    public string nextScene = "Außenwelt";
    private bool hasTransitioned = false;

    void Start() {
        // Scene 2 sofort beim Start additiv laden
        //SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && !hasTransitioned) {
            hasTransitioned = true;
            StartCoroutine(SwitchScene());
        }
    }

    IEnumerator SwitchScene() {
       
    // ERST Player schützen
	DontDestroyOnLoad(GameObject.FindWithTag("Player"));
    
	yield return new WaitForSeconds(0.5f);
    
    // DANN Scene entladen
	SceneManager.UnloadSceneAsync("Tempel");
    
    // Dann teleportieren
	 GameObject.FindWithTag("Player").transform.position = new Vector3(1000, 1, 0);
	}
    
}
