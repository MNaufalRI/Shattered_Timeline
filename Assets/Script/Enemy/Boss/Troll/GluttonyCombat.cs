using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GluttonyCombat : MonoBehaviour
{
    [Header("References")]
    public GluttonyMovement movementScript;
    public Animator anim;
    public Transform player;
    public Transform warningIndicator; 

    [Header("Boss Stats")]
    public float maxHealth = 1000f;
    private float currentHealth;
    public LayerMask playerLayer;

    [Header("Attack 1: Basic (Delay 1s)")]
    public float basicAttackDamage = 15f;
    public float basicAttackRadius = 2.5f;

    [Header("Attack 2: Jump (Delay 2s)")]
    public float jumpDamage = 20f;
    public float jumpRadius = 4f;

    [Header("Attack 3: Gluttony (Delay 1.5s)")]
    public float gluttonyDamage = 10f;
    public float gluttonyRadius = 6f;
    public float gluttonyStunTime = 3f;

    private bool isAttacking = false;

    [Header("Visual Indicators")]
    public Transform circleIndicator; 
    public Transform coneIndicator;

    [Header("Attack Settings")]
    public float basicAttackAngle = 90f;  
    public float gluttonyAttackAngle = 120f;

    void Start()
    {
        currentHealth = maxHealth;
        if (warningIndicator != null) warningIndicator.gameObject.SetActive(false);
        coneIndicator.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isAttacking || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= gluttonyRadius)
        {
            StartCoroutine(ChooseAttack());
        }
    }

    IEnumerator ChooseAttack()
    {
        isAttacking = true;
        movementScript.canMove = false; 

        int randomAttack = Random.Range(0, 3);

        switch (randomAttack)
        {
            case 0: yield return StartCoroutine(BasicAttackRoutine()); break;
            case 1: yield return StartCoroutine(JumpAttackRoutine()); break;
            case 2: yield return StartCoroutine(GluttonySkillRoutine()); break;
        }

        yield return new WaitForSeconds(1.5f); 
        movementScript.canMove = true;
        isAttacking = false;
    }

    // --- 1. BASIC ATTACK ---
    IEnumerator BasicAttackRoutine()
    {
        movementScript.FacePlayerSmooth(0.2f);
        yield return new WaitForSeconds(0.2f);
        anim.SetTrigger("BasicAttackPrepare");

        ShowConeWarning(basicAttackRadius, 1f);

        yield return new WaitForSeconds(1f); 

        anim.SetTrigger("BasicAttackExecute");

        if (IsInFanArea(player, basicAttackRadius, basicAttackAngle))
        {
            DealDamageToPlayer(basicAttackDamage);
        }

        if (Vector3.Distance(transform.position, player.position) <= basicAttackRadius)
        {
            DealDamageToPlayer(basicAttackDamage);
        }
    }

    // --- 2. JUMP SKILL ---
    IEnumerator JumpAttackRoutine()
    {
        movementScript.FacePlayerSmooth(0.2f);
        yield return new WaitForSeconds(0.2f);
        anim.SetTrigger("JumpPrepare");
        Vector3 targetJumpPos = player.position;

        ShowCircleWarning(targetJumpPos, jumpRadius, 2f);

        yield return new WaitForSeconds(2f);

        anim.SetTrigger("JumpExecute");

        transform.DOJump(targetJumpPos, 3f, 1, 0.5f).OnComplete(() =>
        {
            Collider[] hitPlayers = Physics.OverlapSphere(transform.position, jumpRadius, playerLayer);
            foreach (var p in hitPlayers)
            {
                DealDamageToPlayer(jumpDamage);
            }
        });
    }

    // --- 3. GLUTTONY SKILL ---
    IEnumerator GluttonySkillRoutine()
    {
        movementScript.FacePlayerSmooth(0.2f);
        yield return new WaitForSeconds(0.2f);
        anim.SetTrigger("GluttonyPrepare");

        ShowConeWarning(gluttonyRadius, 1.5f);

        yield return new WaitForSeconds(1.5f); 

        anim.SetTrigger("GluttonyExecute");

        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, gluttonyRadius, playerLayer);
        foreach (var p in hitPlayers)
        {
            if (IsInFanArea(p.transform, gluttonyRadius, gluttonyAttackAngle))
            {
                DealDamageToPlayer(gluttonyDamage);

                PlayerStats pStats = p.GetComponent<PlayerStats>();
                if (pStats != null) pStats.ApplyStun(gluttonyStunTime);
            }
        }

        movementScript.ApplySpeedBuff(1.5f, 5f);
    }

    private bool IsInFanArea(Transform target, float radius, float maxAngle)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > radius) return false;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle <= (maxAngle / 2f);
    }

    private void ShowCircleWarning(Vector3 position, float radius, float duration)
    {
        circleIndicator.SetParent(null);
        circleIndicator.position = new Vector3(position.x, 2.72f, position.z);
        circleIndicator.localScale = Vector3.zero;
        circleIndicator.gameObject.SetActive(true);

        circleIndicator.DOScale(new Vector3(radius, 0.01f, radius), duration).OnComplete(() => {
            circleIndicator.gameObject.SetActive(false);
        });
    }

    private void ShowConeWarning(float radius, float duration)
    {
        coneIndicator.SetParent(this.transform);
        coneIndicator.localPosition = new Vector3(0, 0.2f, 1f); 
        coneIndicator.localRotation = Quaternion.Euler(90, 0, 0); 

        coneIndicator.localScale = Vector3.zero;
        coneIndicator.gameObject.SetActive(true);

        // Animasi membesar ke depan
        coneIndicator.DOScale(new Vector3(radius, radius, 1f), duration).OnComplete(() => {
            coneIndicator.gameObject.SetActive(false);
        });
    }

    private void ShowWarningIndicator(Vector3 position, float radius, float duration)
    {
        if (warningIndicator == null)
        {
            Debug.LogError("Warning Indicator belum dipasang di Inspector!");
            return;
        }

        warningIndicator.gameObject.SetActive(true);
        warningIndicator.position = new Vector3(position.x, 2.71f, position.z);

        // Test tanpa animasi: langsung besarkan skalanya
        warningIndicator.localScale = new Vector3(radius, 0.1f, radius);

        Debug.Log("Indikator muncul di: " + warningIndicator.position);

        // Hilangkan setelah durasi habis
        StartCoroutine(HideIndicatorAfter(duration));
    }

    IEnumerator HideIndicatorAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        warningIndicator.gameObject.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        anim.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Debug.Log("Boss Gluttony Defeated!");
            Destroy(gameObject);
        }
    }

    private void DealDamageToPlayer(float damage)
    {
        if (player != null)
        {
            PlayerStats pStats = player.GetComponent<PlayerStats>();
            if (pStats != null)
            {
                pStats.TakeDamage(damage);
            }
        }
    }
}