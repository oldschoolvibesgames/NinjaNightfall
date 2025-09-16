using UnityEngine;

public class AudioSFX : MonoBehaviour
{
    public AudioSource _audioSource;
    
    public void PlayAudio(AudioClip audioClip)
    {
        _audioSource.PlayOneShot(audioClip);
    }
}
