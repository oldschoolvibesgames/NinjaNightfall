using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioVolumeSlider : MonoBehaviour
{
    public enum VolumeType { Music, Sfx }

    [Header("Setup")]
    public AudioMixer mixer;
    public Slider slider;
    public VolumeType volumeType;

    private MMSoundManager _soundManager;

    private void Awake()
    {
        _soundManager = FindAnyObjectByType<MMSoundManager>();
    }

    private void OnEnable()
    {
        if (mixer != null && slider != null)
        {
            string parameter = GetExposedParameter();
            if (mixer.GetFloat(parameter, out float value))
            {
                slider.value = Mathf.Pow(10f, value / 20f);
                slider.onValueChanged.AddListener(OnSliderValueChanged);
            }
        }
    }

    private void OnDisable()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (_soundManager == null) return;

        switch (volumeType)
        {
            case VolumeType.Music:
                _soundManager.SetVolumeMusic(value);
                break;
            case VolumeType.Sfx:
                _soundManager.SetVolumeSfx(value);
                break;
        }

        _soundManager.SaveSettings();
    }

    private string GetExposedParameter()
    {
        return volumeType switch
        {
            VolumeType.Music => "MusicVolume",
            VolumeType.Sfx => "SfxVolume",
            _ => "MasterVolume"
        };
    }
}