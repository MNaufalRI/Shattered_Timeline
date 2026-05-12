using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSync : MonoBehaviour
{
    // Mengambil ID properti dari Shader agar akses lebih cepat daripada menggunakan String
    public static int PosID = Shader.PropertyToID("_Position");
    public static int SizeID = Shader.PropertyToID("_Size");

    public Material WallMaterial; // Masukkan material bangunan kamu di sini
    public Camera Camera;         // Masukkan Main Camera kamu di sini
    public LayerMask Mask;         // Pilih Layer bangunan (misal: "Building")

    void Update()
    {
        // 1. Menghitung arah dari Player ke Kamera
        var dir = Camera.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);

        // 2. Mengecek apakah ada objek di antara Player dan Kamera
        // Jika tertabrak bangunan (Mask), maka lubang (_Size) muncul (1)
        if (Physics.Raycast(ray, 3000, Mask))
            WallMaterial.SetFloat(SizeID, 1);
        else
            WallMaterial.SetFloat(SizeID, 0);

        // 3. Mengubah posisi World Player menjadi Viewport Point (koordinat 0 sampai 1)
        // Ini agar shader tahu koordinat lubangnya di layar monitor
        var view = Camera.WorldToViewportPoint(transform.position);
        WallMaterial.SetVector(PosID, view);
    }
}