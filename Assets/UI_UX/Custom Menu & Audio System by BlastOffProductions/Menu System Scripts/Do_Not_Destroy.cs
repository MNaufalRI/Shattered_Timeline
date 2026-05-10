using UnityEngine;

namespace BlastOffProductions.UI  
{

public class Do_Not_Destroy : MonoBehaviour
{
    [Header("Persistence Settings")]
    [Tooltip("If true, this GameObject will persist between scene loads.")]
    public bool toggleDontDestroyScript = true;

    void Awake()
    {
        if (toggleDontDestroyScript)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
}
