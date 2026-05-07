using StarterAssets;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Animation")]
    [SerializeField] private Animator anim;

    [Header("Movement")]
    [SerializeField] private PlayerMovement2 movement;

    private Interactable currentInteractable;
    private bool isInteracting = false;

    void Awake()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (movement == null)
            movement = GetComponent<PlayerMovement2>();
    }

    void Update()
    {
        FindNearestInteractable();
    }

    public void OnInteract()
    {
        if (currentInteractable == null || isInteracting) return;

        isInteracting = true;

        if (movement != null)
            movement.canMove = false;

        anim.SetBool("isGathering", true);
    }

    public void FinishGather()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }

        anim.SetBool("isGathering", false);

        if (movement != null)
            movement.canMove = true;

        isInteracting = false;
    }

    void FindNearestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactRange,
            interactableLayer
        );

        float closestDistance = Mathf.Infinity;
        Interactable nearest = null;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Interactable interactable))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    nearest = interactable;
                }
            }
        }

        currentInteractable = nearest;
    }
}