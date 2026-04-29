using UnityEngine;
using System.Collections;

public class GatherableItem : Interactable
{
    [Header("Gathering Settings")]
    public string itemName = "Tanaman Herbal"; 
    public float gatherTime = 2.1f;

    private bool isGathering = false; 
    void Start()
    {
        promptMessage = "untuk mengumpulkan " + itemName;
    }

    public override void Interact()
    {
        if (!isGathering)
        {
            StartCoroutine(GatherRoutine());
        }
    }

    IEnumerator GatherRoutine()
    {
        isGathering = true;
        Player_Controlled_2 playerScript = FindFirstObjectByType<Player_Controlled_2>();

        if (playerScript != null)
        {
            playerScript.canControl = false;

            playerScript.anim.Play("Gathering");

            Debug.Log("Animasi Gathering dimulai...");
        }

        yield return new WaitForSeconds(gatherTime);

        if (playerScript != null)
        {
            playerScript.canControl = true;
        }

#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == gameObject)
        {
            UnityEditor.Selection.activeGameObject = null;
        }
#endif
        Destroy(gameObject);
    }
}