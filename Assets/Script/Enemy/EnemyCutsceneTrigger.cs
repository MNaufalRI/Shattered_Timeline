using UnityEngine;
using Unity.Cinemachine; 
using System.Collections;

public class EnemyCutsceneTrigger : MonoBehaviour
{
    public CinemachineCamera enemyVcam; 
    public float cutsceneDuration = 3f;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            Player_Controlled_2 playerScript = other.GetComponent<Player_Controlled_2>();

            if (playerScript != null)
            {
                StartCoroutine(PlayCutscene(playerScript));
            }
        }
    }

    IEnumerator PlayCutscene(Player_Controlled_2 player)
    {
        hasTriggered = true;

        player.canControl = false;

        enemyVcam.Priority = 20;
        yield return new WaitForSeconds(cutsceneDuration);

        enemyVcam.Priority = 1;

        player.canControl = true;

        Destroy(gameObject);
    }
}