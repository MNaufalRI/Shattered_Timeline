using System.Collections;
using UnityEngine;

public class BossArenaTrigger : MonoBehaviour
{
    [Header("Boss References")]
    public BossHealthUI bossUIManager;
    public DragonBoarStats bossTarget;
    public DragonBoarCombat bossCombat;
    public Animator bossAnimator;

    [Header("Cutscene Elements")]
    public GameObject cutsceneCamera; 
    public GameObject invisibleWall;  
    public float cutsceneDuration = 4f; 

    [Header("Player References")]
    public MonoBehaviour playerMovement; 
    public MonoBehaviour playerCombat;  

    [Header("UI Elements")]
    public CanvasGroup mainHUDCanvasGroup; 
    public float uiFadeSpeed = 2f;

    private bool hasTriggered = false;

    private void Start()
    {
        if (bossTarget != null) bossTarget.enabled = false;
        if (cutsceneCamera != null) cutsceneCamera.SetActive(false);

        if (invisibleWall != null) invisibleWall.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            // Jika bos sudah Phase 2, LANGSUNG mulai perang tanpa cutscene panjang
            if (bossTarget != null && bossTarget.isPhase2)
            {
                StartCoroutine(SkipCutsceneSequence());
            }
            else
            {
                // Jika masih Phase 1, mainkan cutscene normal
                StartCoroutine(PlayCutsceneSequence());
            }
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
        Debug.Log("<color=cyan>Cutscene Started!</color>");

        if (mainHUDCanvasGroup != null) mainHUDCanvasGroup.alpha = 0f;

        if (invisibleWall != null) invisibleWall.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;

        if (cutsceneCamera != null) cutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (bossAnimator != null) bossAnimator.SetTrigger("Roar");

        yield return new WaitForSeconds(cutsceneDuration);

        if (cutsceneCamera != null) cutsceneCamera.SetActive(false);
        yield return new WaitForSeconds(2f);

        if (mainHUDCanvasGroup != null) mainHUDCanvasGroup.alpha = 1f;
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;

        if (bossUIManager != null && bossTarget != null)
        {
            bossUIManager.ActivateBossUI(bossTarget);
        }

        if (bossCombat != null)
        {
            bossCombat.isActivated = true;
            UnityEngine.AI.NavMeshAgent agent = bossCombat.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.isStopped = false;
        }

        Debug.Log($"<color=orange>Boss Battle Started: {bossTarget.name}</color>");
    }

    public void ResetTrigger()
    {
        hasTriggered = false; 

        if (invisibleWall != null)
        {
            invisibleWall.SetActive(false); 
        }

        if (cutsceneCamera != null)
        {
            cutsceneCamera.SetActive(false);
        }

        Debug.Log("<color=yellow>Boss Arena Trigger Ready For Re-Entry!</color>");
    }
    private IEnumerator SkipCutsceneSequence()
    {
        if (invisibleWall != null) invisibleWall.SetActive(true);

        if (bossUIManager != null && bossTarget != null) bossUIManager.ActivateBossUI(bossTarget);

        if (bossCombat != null)
        {
            bossCombat.isActivated = true;
            UnityEngine.AI.NavMeshAgent agent = bossCombat.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.isStopped = false;
        }

        yield return null;
    }
}