using UnityEngine;

public class PlayerPersist : MonoBehaviour {

    void Start() {
        DontDestroyOnLoad(gameObject); // = Player
        DontDestroyOnLoad(Camera.main.gameObject); // = Kamera
    }
}
