using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private GatherableItem currentItem;
    private Animator anim;
    private PlayerMovement movement;

    void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    public void StartGathering(GatherableItem item)
    {
        currentItem = item;

        movement.canControl = false;
        anim.SetBool("isGathering", true);
    }

    public void FinishGathering()
    {
        anim.SetBool("isGathering", false);
        movement.canControl = true;

        if (currentItem != null)
        {
            currentItem.OnGatherFinished();
            currentItem = null;
        }
    }
}