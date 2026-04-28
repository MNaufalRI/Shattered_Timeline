using UnityEngine;
using Unity.Cinemachine; // Namespace baru untuk Unity 6
using System.Collections;

public class EnemyCutsceneTrigger : MonoBehaviour
{
    public CinemachineCamera enemyVcam; // Tarik Vcam_Enemy ke sini
    public float cutsceneDuration = 3f;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Cek jika yang masuk adalah Player dan belum pernah trigger sebelumnya
        if (other.CompareTag("Player") && !hasTriggered)
        {
            StartCoroutine(PlayCutscene());
        }
    }

    IEnumerator PlayCutscene()
    {
        hasTriggered = true;

        // 1. Aktifkan Kamera Musuh dengan menaikkan Prioritas
        enemyVcam.Priority = 20;

        // 2. Tunggu selama durasi yang diinginkan
        yield return new WaitForSeconds(cutsceneDuration);

        // 3. Kembalikan ke Kamera Player dengan menurunkan Prioritas
        enemyVcam.Priority = 1;

        // Opsional: Hancurkan trigger ini agar tidak terulang
        // Destroy(gameObject);
    }
}