using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    public float destroyTime = 2.0f; // Sesuaikan dengan durasi VFX-mu

    void Start()
    {
        // Menghapus objek ini secara otomatis setelah X detik
        Destroy(gameObject, destroyTime);
    }
}