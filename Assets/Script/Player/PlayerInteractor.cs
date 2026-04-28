using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerInteractor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private LayerMask interactableLayer;

    private Interactable currentInteractable;

    void Update()
    {
        FindNearestInteractable();

        if (currentInteractable != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            currentInteractable.Interact();
        }
    }

    void FindNearestInteractable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);

        if (hitColliders.Length > 0)
        {
            if (hitColliders[0].TryGetComponent<Interactable>(out Interactable interactable))
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    Debug.Log("Tekan F " + currentInteractable.promptMessage);
                }
                return;
            }
        }

        currentInteractable = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}