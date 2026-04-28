using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable Settings")]
    public string promptMessage = "untuk berinteraksi"; 

    public abstract void Interact();
}