using UnityEngine;
using UnityEngine.UI;

namespace BlastOffProductions.UI 
{

public class UIAnimationDisabler : MonoBehaviour
{
    [Header("Assign your UI element here")]
    public GameObject uiElement;               // The UI element to animate
    public string animationTrigger = "Play";   // Trigger name in Animator

    [Header("Audio Settings")]
    [Tooltip("Sound to play when the button is clicked/selected.")]
    public AudioClip selectSound;

    [Tooltip("AudioSource used to play the sound.")]
    public AudioSource audioSource;

    private Animator animator;

    void Start()
    {
        if (uiElement != null)
            animator = uiElement.GetComponent<Animator>();
    }

    /// <summary>
    /// Plays the animation on the UI element and a sound when called.
    /// </summary>
    public void PlayAnimation()
    {
        if (uiElement == null || animator == null)
        {
            Debug.LogWarning("UI Element or Animator is missing!");
            return;
        }

        // Play sound if assigned
        if (audioSource != null && selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }

        // Make sure the element is active
        if (!uiElement.activeSelf)
            uiElement.SetActive(true);

        // Trigger the animation
        animator.ResetTrigger(animationTrigger);
        animator.SetTrigger(animationTrigger);
    }
}
}
