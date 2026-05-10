using UnityEngine;
using UnityEngine.UI;

namespace BlastOffProductions.UI  
{

public class UIAnimationEnabler : MonoBehaviour
{
    [Header("Assign these in the Inspector")]
    public Button triggerButton;      // Button you click
    public GameObject uiElement;      // UI element to enable + animate
    public string animationTrigger = "Play"; // Animator trigger name

    [Header("Audio Settings")]
    [Tooltip("Sound to play when the button is clicked.")]
    public AudioClip clickSound;

    [Tooltip("AudioSource used to play the sound.")]
    public AudioSource audioSource;

    private Animator animator;

    void Start()
    {
        if (uiElement != null)
            animator = uiElement.GetComponent<Animator>();

        if (triggerButton != null)
            triggerButton.onClick.AddListener(EnableAndPlay);
    }

    /// <summary>
    /// Enables the UI element, plays the animation, and plays a sound
    /// </summary>
    public void EnableAndPlay()
    {
        if (uiElement == null)
        {
            Debug.LogWarning("UI Element not assigned!");
            return;
        }

        // Enable the element if it was disabled
        if (!uiElement.activeSelf)
            uiElement.SetActive(true);

        // Trigger the animation
        if (animator == null)
            animator = uiElement.GetComponent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger(animationTrigger);
            animator.SetTrigger(animationTrigger);
        }
        else
        {
            Debug.LogWarning("No Animator found on " + uiElement.name);
        }

        // Play the click sound
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
}
