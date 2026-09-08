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
    private bool hasCutscenePlayed = false;

    public BossQuestManager questManager;

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

            if (questManager != null)
            {
                questManager.StartBossFight();
            }

            if (bossTarget != null && bossTarget.isPhase2)
            {
                StartCoroutine(SkipCutsceneSequence());
            }
            else if (!hasCutscenePlayed)
            {
                hasCutscenePlayed = true;
                StartCoroutine(PlayCutsceneSequence());
            }
            else
            {
                StartCoroutine(SkipCutsceneSequence());
            }
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
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