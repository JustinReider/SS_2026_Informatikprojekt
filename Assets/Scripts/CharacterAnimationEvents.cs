using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    [SerializeField] private AudioSource pickaxeAudioSource;

    public void PlayPickaxeSound()
    {
        pickaxeAudioSource.Stop();
        pickaxeAudioSource.Play();
    }
}