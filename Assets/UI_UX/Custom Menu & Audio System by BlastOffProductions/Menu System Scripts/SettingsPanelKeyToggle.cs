using UnityEngine;

namespace BlastOffProductions.UI  
{

public class SettingsPanelKeyAnimator : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Animator on the Settings panel (or a child).")]
    public Animator settingsAnimator;

    [Header("Keyboard")]
    [Tooltip("Key that toggles the panel animations.")]
    public KeyCode toggleKey = KeyCode.P;   // pick any letter/key in Inspector

    [Header("Animator Triggers")]
    [Tooltip("Trigger to play the 'appear/show' animation.")]
    public string showTrigger = "Show";
    [Tooltip("Trigger to play the 'hide/disappear' animation.")]
    public string hideTrigger = "Hide";

    [Header("State")]
    [Tooltip("Starting state of the panel (true = open).")]
    public bool isOpenAtStart = false;

    [Header("Audio Settings")]
    [Tooltip("Sound to play when the toggle key is pressed.")]
    public AudioClip toggleSound;
    [Tooltip("AudioSource used to play the sound.")]
    public AudioSource audioSource;

    private bool _isOpen;

    void Awake()
    {
        _isOpen = isOpenAtStart;

        // Optional: nudge Animator into a known state on start.
        if (settingsAnimator != null && Application.isPlaying)
        {
            if (_isOpen && !string.IsNullOrEmpty(showTrigger))
                settingsAnimator.ResetTrigger(showTrigger);
            if (!_isOpen && !string.IsNullOrEmpty(hideTrigger))
                settingsAnimator.ResetTrigger(hideTrigger);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    /// <summary>
    /// Toggles the panel by playing the appropriate animation trigger and sound.
    /// </summary>
    public void Toggle()
    {
        if (settingsAnimator == null)
        {
            Debug.LogWarning("[SettingsPanelKeyAnimator] No Animator assigned.");
            return;
        }

        // Play toggle sound if available
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }

        if (_isOpen)
        {
            if (!string.IsNullOrEmpty(hideTrigger))
            {
                settingsAnimator.ResetTrigger(showTrigger);
                settingsAnimator.SetTrigger(hideTrigger);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(showTrigger))
            {
                settingsAnimator.ResetTrigger(hideTrigger);
                settingsAnimator.SetTrigger(showTrigger);
            }
        }

        _isOpen = !_isOpen;
    }
}
}