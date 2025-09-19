using MoreMountains.Tools;
using UnityEngine;

public class DisableMMSound : MonoBehaviour
{
    private MMSoundManager _mmSound;
    
    private void OnEnable()
    {
        _mmSound = FindAnyObjectByType<MMSoundManager>();
        if(_mmSound != null) _mmSound.PauseAllSounds();
    }
    
    private void OnDisable()
    {
        _mmSound = FindAnyObjectByType<MMSoundManager>();
        if(_mmSound != null) _mmSound.PlayAllSounds();
    }
}
