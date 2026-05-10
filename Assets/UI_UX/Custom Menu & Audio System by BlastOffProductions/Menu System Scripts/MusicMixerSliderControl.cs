using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace BlastOffProductions.UI  
{

public class MusicMixerSliderControl : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public AudioMixer audioMixer;                 // Drag your AudioMixer asset here
    public string mixerExposedParameter = "SFX Control Variable"; // Exposed parameter name
    public Slider slider;                         // Drag your UI Slider here

    [Header("Slider Range Settings")]
    [Tooltip("Minimum slider value in dB")]
    public float minValue = -80f;
    [Tooltip("Maximum slider value in dB")]
    public float maxValue = 0f;

    void Start()
    {
        if (slider != null)
        {
            // Sync slider with current mixer value
            float currentValue;
            if (audioMixer.GetFloat(mixerExposedParameter, out currentValue))
            {
                slider.minValue = minValue;
                slider.maxValue = maxValue;
                slider.value = currentValue;
            }

            slider.onValueChanged.AddListener(SetMixerVolume);
        }
    }

    /// <summary>
    /// Updates the AudioMixer parameter when the slider changes.
    /// </summary>
    /// <param name="value">Slider value in dB</param>
    public void SetMixerVolume(float value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(mixerExposedParameter, value);
        }
    }
}
}
