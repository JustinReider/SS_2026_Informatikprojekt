using UnityEngine;
using System.Collections;

public class DelayedSpawner : MonoBehaviour
{
    public GameObject character;
    public float spawnDelay = 3f;

    void Start()
    {
        character.SetActive(false);
        StartCoroutine(SpawnWithDelay());
    }

    private IEnumerator SpawnWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        character.SetActive(true);
    }
}