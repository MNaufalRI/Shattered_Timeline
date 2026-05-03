using UnityEngine;

public class GatherableItem : Interactable
{
    [Header("Gathering Settings")]
    public string itemName = "Tanaman Herbal";

    private bool isGathering = false;

    void Start()
    {
        promptMessage = "untuk mengumpulkan " + itemName;
    }

    public override void Interact()
    {
        if (isGathering) return;

        Player_Controlled_3 player = FindFirstObjectByType<Player_Controlled_3>();
        if (player == null) return;

        isGathering = true;

        player.StartGathering(this);

        Debug.Log("Mulai gathering...");
    }

    public void OnGatherFinished()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == gameObject)
        {
            UnityEditor.Selection.activeGameObject = null;
        }
#endif

        Destroy(gameObject);
    }
}