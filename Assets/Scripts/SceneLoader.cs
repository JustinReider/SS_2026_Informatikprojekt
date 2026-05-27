using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class mover : MonoBehaviour {


    void Update() {

	
    }
    
    private void OnTriggerEnter(Collider other) {
    if (other.CompareTag("TempleToWorld")) {  // CompareTag ist besser als .tag ==
        SceneManager.LoadScene(1);
    }
}

// Zusätzlich OnTriggerStay als Fallback:
private void OnTriggerStay(Collider other) {
    if (other.CompareTag("TempleToWorld")) {
        SceneManager.LoadScene(1);
    }
}


}

