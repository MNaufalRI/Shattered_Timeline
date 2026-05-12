using UnityEngine;
using System.Collections.Generic;

public class CameraOcclusionRaycast : MonoBehaviour
{
    [Header("Targets")]
    public Transform playerTransform; // Tarik objek Player ke sini

    [Header("Settings")]
    public LayerMask obstructingLayer; // Set layer bangunan/gedung di sini
    [Range(0f, 1f)]
    public float fadedOpacity = 0.2f; // Tingkat keburaman saat tembus pandang
    public float fadeSpeed = 5f; // Kecepatan transisi memudar

    // Untuk menyimpan referensi material dan target opacity saat ini
    private Dictionary<Renderer, float> targetOpacities = new Dictionary<Renderer, float>();
    private List<Renderer> renderersToRemove = new List<Renderer>();

    // Nama variabel di Shader Graph harus sama persis (termasuk huruf besar/kecil)
    private const string shaderOpacityParam = "_Opacity";

    void Update()
    {
        if (playerTransform == null) return;

        // 1. Lakukan Raycasting
        PerformRaycast();

        // 2. Haluskan transisi Opacity (Lerp)
        UpdateMaterialProperties();
    }

    void PerformRaycast()
    {
        Vector3 direction = playerTransform.position - transform.position;
        float distance = direction.magnitude;

        // Tembakkan Ray dari kamera ke arah player
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance, obstructingLayer);

        // Tandai semua objek yang terkena Raycast untuk memudar
        HashSet<Renderer> currentHits = new HashSet<Renderer>();
        foreach (var hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                currentHits.Add(rend);
                if (!targetOpacities.ContainsKey(rend))
                {
                    // Jika baru pertama kena, tambahkan ke dictionary dengan opacity penuh
                    targetOpacities.Add(rend, 1f);
                }
                // Set target ke buram
                targetOpacities[rend] = fadedOpacity;
            }
        }

        // Cari objek yang sudah tidak menghalangi dan set target kembali ke penuh (1)
        renderersToRemove.Clear();
        foreach (var rend in targetOpacities.Keys)
        {
            if (!currentHits.Contains(rend))
            {
                renderersToRemove.Add(rend);
            }
        }

        // Set target kembali ke 1 untuk objek yang sudah clear
        foreach (var rend in renderersToRemove)
        {
            targetOpacities[rend] = 1f;
        }
    }

    void UpdateMaterialProperties()
    {
        // Gunakan list sementara untuk iterasi agar bisa menghapus item saat looping
        List<Renderer> keys = new List<Renderer>(targetOpacities.Keys);

        foreach (var rend in keys)
        {
            if (rend == null)
            {
                targetOpacities.Remove(rend);
                continue;
            }

            // Ambil nilai opacity saat ini dari material
            float currentVal = rend.material.GetFloat(shaderOpacityParam);
            float targetVal = targetOpacities[rend];

            // Haluskan nilai menuju target
            float newVal = Mathf.Lerp(currentVal, targetVal, Time.deltaTime * fadeSpeed);

            // Terapkan kembali ke material
            rend.material.SetFloat(shaderOpacityParam, newVal);

            // Jika sudah kembali penuh (1) dan stabil, hapus dari dictionary untuk performa
            if (targetVal == 1f && Mathf.Approximately(newVal, 1f))
            {
                // Kembalikan ke persis 1 agar tidak ada sisa dither
                rend.material.SetFloat(shaderOpacityParam, 1f);
                targetOpacities.Remove(rend);
            }
        }
    }
}