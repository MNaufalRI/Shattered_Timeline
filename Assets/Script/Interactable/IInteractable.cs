using UnityEngine;

public class IInteracable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public interface IInteractable
    {
        void Interact(PlayerInteraction player);
    }
}
