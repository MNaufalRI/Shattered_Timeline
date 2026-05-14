using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Tarik objek PlayerController ke sini")]
    public GameObject Player;

    [Header("Camera Height Settings")]
    [Tooltip("Atur ketinggian kamera minimap di sini")]
    public float cameraHeight = 40f;

    private void LateUpdate()
    {
        // Pastikan variabel Player sudah diisi di Inspector agar tidak error
        if (Player != null)
        {
            // Menggunakan variabel 'cameraHeight' alih-alih angka 40 yang dikunci (hardcoded)
            transform.position = new Vector3(Player.transform.position.x, cameraHeight, Player.transform.position.z);
        }
    }
}