using UnityEngine;

public class AnimationSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource AudioSource;

    public void PlaySound()
    {
        AudioSource.Stop();
        AudioSource.Play();
    }
}
