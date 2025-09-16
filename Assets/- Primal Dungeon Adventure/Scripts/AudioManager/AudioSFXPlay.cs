using UnityEngine;

public class AudioSFXPlay : MonoBehaviour
{
    public AudioClip[] clip;
    public bool doOnce;
    private bool _done;
    private AudioSFX _sfx;

    private void OnEnable()
    {
        _sfx = FindAnyObjectByType<AudioSFX>();
    }

    public void PlayAudioByIndex(int index)
    {
        if(_done) return;
        
        if(_sfx == null) _sfx = FindAnyObjectByType<AudioSFX>();
        _sfx.PlayAudio(clip[index]);
        if (doOnce) _done = true;
    }
    
    public void PlayAudioByClip(AudioClip newClip)
    {
        if(_done) return;
        
        if(_sfx == null) _sfx = FindAnyObjectByType<AudioSFX>();
        _sfx.PlayAudio(newClip);
        if (doOnce) _done = true;
    }
}
