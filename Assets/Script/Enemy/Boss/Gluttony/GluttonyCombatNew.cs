using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DragonBoarStats))]
[RequireComponent(typeof(NavMeshAgent))]
public class DragonBoarCombat : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    private PlayerStats playerStats;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float basicAttackDamage = 5f;
    public float heavyAttackDamage = 10f;

    [Header("Heavy Attack (High Speed Pursuit)")]
    public float chargeSpeed = 12f;
    public float rageChargeSpeed = 25f; 
    public float forceHeavyAttackTime = 5f; 
    private float normalSpeed;

    [Header("Gluttony Settings")]
    public float gluttonyAnimDuration = 3f;
    public float extraStunTime = 4f;
    public float pullSpeed = 5f; 
    public float suckDuration = 2f; 
    public float maxDistanceTrigger = 15f;
    public float autoSkillCooldown = 15f; 
    public float gluttonyAutoCooldown = 20f; 
    private float gluttonyAutoTimer = 0f;
    public Transform gluttonyPullTarget;

    private float gameTimer = 0f;

    [Header("Cooldowns")]
    public float attackCooldown = 2f;
    private float attackTimer = 0f;
    public float timeSinceLastAttack = 0f;

    private Animator anim;
    private NavMeshAgent agent;
    private DragonBoarStats stats;

    [HideInInspector] public bool isAttacking = false;
    private float normalAngularSpeed;
    private float normalAcceleration;

    [Header("Rotten Expulsion (Acid Meteor)")]
    public Transform mouthTransform; 
    public GameObject projectileUpPrefab; 
    public GameObject acidMeteorPrefab; 
    public int meteorCount = 3;
    public float meteorDelay = 0.5f;

    [Header("Attack Probability (Total must be 100)")]
    [Range(0f, 100f)] public float basicAttackChance = 35f;
    [Range(0f, 100f)] public float heavyAttackChance = 30f;
    [Range(0f, 100f)] public float gluttonyChance = 15f;
    [Range(0f, 100f)] public float rottenExpulsionChance = 20f;

    [Header("VFX Settings")]
    public GameObject gluttonyVFXPrefab; 
    public Transform GluttonyTransform;    
    public GameObject activeVFX;      
    public GameObject heavyAttackVFXPrefab; 
    public Transform heavyAttackVFXTransform;

    private bool isStealingSpeed = false; 
    public bool isActivated = false;

    [Header("Cutscene UI")]
    public CanvasGroup bossTitleCanvasGroup; 
    public float fadeDuration = 1f;

    private void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<DragonBoarStats>();

        normalSpeed = agent.speed;
        normalAngularSpeed = agent.angularSpeed;
        normalAcceleration = agent.acceleration;

        if (!isActivated)
        {
            agent.isStopped = true;
        }

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    private void Update()
    {
        if (stats.isDead || player == null || (playerStats != null && playerStats.isDead))
        {
            if (isAttacking)
            {
                ForceStopGluttonyVFX();
                StopAllCoroutines();    
                ResetAttack();          
            }
            return;
        }
        if (!isActivated) return;

        gameTimer += Time.deltaTime;
        if (gluttonyAutoTimer > 0) gluttonyAutoTimer -= Time.deltaTime;
        if (isAttacking) return;

        timeSinceLastAttack += Time.deltaTime;
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (gameTimer >= 15f && distanceToPlayer > maxDistanceTrigger && gluttonyAutoTimer <= 0)
        {
            StartCoroutine(GluttonyRoutine());
            return;
        }

        if (timeSinceLastAttack >= forceHeavyAttackTime)
        {
            StartCoroutine(HeavyAttackRoutine(true));
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            anim.SetFloat("Speed", 0f);
            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);

            if (attackTimer <= 0f)
            {
                ChooseRandomAttack();
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    public void ShowBossTitle()
    {
        if (bossTitleCanvasGroup != null)
        {
            StartCoroutine(FadeTitle(0f, 1f));
            Debug.Log("Boss Title Fading In...");
        }
    }

    public void HideBossTitle()
    {
        if (bossTitleCanvasGroup != null)
        {
            StartCoroutine(FadeTitle(1f, 0f));
            Debug.Log("Boss Title Fading Out...");
        }
    }

    private IEnumerator FadeTitle(float startAlpha, float targetAlpha)
    {
        float timer = 0f;
        bossTitleCanvasGroup.alpha = startAlpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            bossTitleCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        bossTitleCanvasGroup.alpha = targetAlpha;
    }

    private void ChooseRandomAttack()
    {

        float totalChance = basicAttackChance + heavyAttackChance + rottenExpulsionChance;

        if (gluttonyAutoTimer <= 0)
        {
            totalChance += gluttonyChance;
        }

        float rand = Random.Range(0f, totalChance);

        if (rand < basicAttackChance)
        {
            StartCoroutine(BasicAttackRoutine());
        }
        else if (rand < basicAttackChance + heavyAttackChance)
        {
            StartCoroutine(HeavyAttackRoutine(false));
        }
        else if (gluttonyAutoTimer <= 0 && rand < basicAttackChance + heavyAttackChance + gluttonyChance)
        {
            StartCoroutine(GluttonyRoutine());
        }
        else
        {
            StartCoroutine(RottenExpulsionRoutine());
        }
    }

    private IEnumerator BasicAttackRoutine()
    {
        isAttacking = true;
        timeSinceLastAttack = 0f;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        anim.SetTrigger("BasicAttack");

        yield return new WaitForSeconds(0.5f);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            DealDamageToPlayer(basicAttackDamage);
        }
    }
    private IEnumerator HeavyAttackRoutine(bool isRageMode)
    {
        isAttacking = true;
        timeSinceLastAttack = 0f;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = isRageMode ? rageChargeSpeed : chargeSpeed;
            agent.angularSpeed = 2000f;
            agent.acceleration = 100f;
            agent.isStopped = false;
        }

        bool hasTriggeredAnim = false;
        float chaseTimeout = 5f;

        while (Vector3.Distance(transform.position, player.position) > attackRange && chaseTimeout > 0)
        {
            chaseTimeout -= Time.deltaTime;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
                anim.SetFloat("Speed", agent.velocity.magnitude);
            }

            if (!hasTriggeredAnim && Vector3.Distance(transform.position, player.position) <= attackRange + 1.5f)
            {
                anim.SetTrigger("HeavyAttack");
                hasTriggeredAnim = true;
                break;
            }
            yield return null;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        anim.SetFloat("Speed", 0f);

        if (!hasTriggeredAnim)
        {
            anim.SetTrigger("HeavyAttack");
            hasTriggeredAnim = true;
        }

        float windUpTimer = 0.8f;
        while (windUpTimer > 0)
        {
            if (isRageMode && player != null)
            {
                Vector3 lookPos = player.position - transform.position;
                lookPos.y = 0;
                if (lookPos != Vector3.zero)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 10f);

                transform.position = Vector3.MoveTowards(transform.position, player.position, Time.deltaTime * 2f);
            }
            windUpTimer -= Time.deltaTime;
            yield return null;
        }

        Vector3 impactPoint = (heavyAttackVFXTransform != null) ? heavyAttackVFXTransform.position : transform.position + transform.forward * 2f;

        float coneRadius = isRageMode ? 10f : 9f; 
        float coneAngle = isRageMode ? 75f : 80f;

        Collider[] hitColliders = Physics.OverlapSphere(impactPoint, coneRadius);
        bool playerInCone = false;

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                playerInCone = true;
                break;
            }
        }


        if (playerInCone || isRageMode) 
        {

            float damageToDeal = isRageMode ? 15f : 10f;
            float stunDuration = isRageMode ? 2f : 1f;

            DealDamageToPlayer(damageToDeal);

            if (playerStats != null)
            {
                playerStats.ApplyStun(stunDuration);
            }

            Debug.Log($"<color=red>Heavy Attack Hit! Mode Rage: {isRageMode}, Damage: {damageToDeal}</color>");
        }

        yield break;
    }

    public void SpawnHeavyAttackVFX()
    {
        if (stats.isImmune || stats.isPhase2 && !stats.isPhase2Ready)
        {
            return;
        }

        if (heavyAttackVFXPrefab != null && heavyAttackVFXTransform != null)
        {
            GameObject vfx = Instantiate(heavyAttackVFXPrefab, heavyAttackVFXTransform.position, heavyAttackVFXTransform.rotation);

            Destroy(vfx, 3f);
        }

        Debug.Log("<color=yellow>Heavy Attack VFX Triggered!</color>");
    }

    private IEnumerator GluttonyRoutine()
    {
        isAttacking = true;
        timeSinceLastAttack = 0f; 
        gluttonyAutoTimer = gluttonyAutoCooldown;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; 
        }

        if (player != null)
        {
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }

        anim.SetTrigger("Gluttony");
        yield return new WaitForSeconds(1f);
        if (gluttonyVFXPrefab != null && GluttonyTransform != null)
        {
            activeVFX = Instantiate(gluttonyVFXPrefab, GluttonyTransform.position, GluttonyTransform.rotation);
            activeVFX.transform.parent = GluttonyTransform;
        }

        PlayerMovement2 playerMove = player.GetComponent<PlayerMovement2>();
        CharacterController controller = player.GetComponent<CharacterController>();

        if (playerMove != null)
        {
            playerMove.MoveSpeed *= 0.2f;
            playerMove.SprintSpeed *= 0.2f;
            playerMove.enabled = false;
        }
        float timer = 0f;
        while (timer < suckDuration)
        {
            if (playerStats != null && playerStats.isDead) break;
            timer += Time.deltaTime;
            if (player != null && controller != null)
        {
            Vector3 pullTargetPos = (gluttonyPullTarget != null) ? gluttonyPullTarget.position : transform.position;

  
            Vector3 pullDirection = (pullTargetPos - player.position).normalized;
            pullDirection.y = 0; 

 
            float distanceToTarget = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), 
                                                    new Vector3(pullTargetPos.x, 0, pullTargetPos.z));
            if (distanceToTarget > 1.5f)
            {
                controller.Move(pullDirection * (pullSpeed * Time.deltaTime));
            }
        }
        yield return null;
        }

        if (playerStats != null && playerStats.isDead)
        {
            ForceStopGluttonyVFX();
            yield break; 
        }

        if (activeVFX != null)
        {
            ParticleSystem ps = activeVFX.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();
            Destroy(activeVFX, 4f); 
        }

        if (playerStats != null) playerStats.ApplyStun(3f);
        yield return new WaitForSeconds(1f);

        if (stats != null && stats.isPhase2)
        {
            StartCoroutine(GluttonySpeedStealRoutine(playerMove));
        }
        else
        {
            if (playerMove != null)
            {
                playerMove.enabled = true;
                playerMove.MoveSpeed /= 0.2f;
                playerMove.SprintSpeed /= 0.2f;
            }
        }

    }

    private IEnumerator GluttonySpeedStealRoutine(PlayerMovement2 playerMove)
    {
    
        if (isStealingSpeed) yield break;
        isStealingSpeed = true;

       
        if (playerMove != null)
        {
            playerMove.enabled = true; 

           
            playerMove.MoveSpeed = (playerMove.MoveSpeed / 0.2f) * 0.6f;
            playerMove.SprintSpeed = (playerMove.SprintSpeed / 0.2f) * 0.6f;
        }

    
        float originalSpeedBeforeSteal = normalSpeed;

      
        normalSpeed *= 1.4f;
        if (agent != null) agent.speed = normalSpeed;

        Debug.Log("<color=orange>Gluttony Speed Steal Active! (5 Seconds)</color>");

    
        yield return new WaitForSeconds(5f);

   
        if (playerMove != null)
        {
            playerMove.MoveSpeed /= 0.6f;
            playerMove.SprintSpeed /= 0.6f;
        }

        normalSpeed = originalSpeedBeforeSteal;
        if (!isAttacking && agent != null) agent.speed = normalSpeed;

        isStealingSpeed = false;
        Debug.Log("<color=cyan>Gluttony Speed Steal Ended.</color>");
    }

    private void DealDamageToPlayer(float damageAmount)
    {
        if (playerStats != null && !playerStats.isInvincible)
        {
            playerStats.TakeDamage(damageAmount);
        }
    }

    public void ResetAttack()
    {
        isAttacking = false;

    
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.speed = normalSpeed;
            agent.angularSpeed = normalAngularSpeed;
            agent.acceleration = normalAcceleration;
            agent.isStopped = false;
        }

        attackTimer = attackCooldown;
        timeSinceLastAttack = 0f;

        if (anim != null)
        {
            anim.SetBool("isAttacking", false);
        }

        Debug.Log("<color=green>Attack Reset via Animation Event!</color>");
    }

    private IEnumerator RottenExpulsionRoutine()
    {
        isAttacking = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        anim.SetTrigger("SpitAcid");
        yield break;
    }

    public void SpawnAcidSpit()
{
        if (stats.isImmune || stats.isPhase2 && !stats.isPhase2Ready)
        {
            return;
        }
        if (projectileUpPrefab != null && mouthTransform != null)
    {
       
        GameObject spit = Instantiate(projectileUpPrefab, mouthTransform.position, mouthTransform.rotation);
        Destroy(spit, 1f);
    }


    StartCoroutine(SpawnMeteorsSequence());
    
    Debug.Log("<color=green>Acid Spit VFX Triggered via Animation Event!</color>");
}

    private IEnumerator SpawnMeteorsSequence()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < meteorCount; i++)
        {
            if (player != null)
            {
                Vector3 spawnPos = player.position;
                Instantiate(acidMeteorPrefab, spawnPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(meteorDelay);
        }
    }

    public void ApplyPhase2CombatBuffs()
    {
 
        meteorCount += 1;

        basicAttackDamage *= 1.2f;
        heavyAttackDamage *= 1.2f;
        normalSpeed *= 1.3f;
        chargeSpeed *= 1.3f;    
        rageChargeSpeed *= 1.3f;

        if (!isAttacking && agent != null)
        {
            agent.speed = normalSpeed;
        }

        Debug.Log("<color=yellow>Phase 2 Combat Buffs Applied! Damage, Speed, and Meteor increased.</color>");
    }

    public void ForceStopGluttonyVFX()
    {
        if (activeVFX != null)
        {
            Destroy(activeVFX);
            activeVFX = null;
        }
    }

    private void OnDisable()
    {
        ForceStopGluttonyVFX();
    }

}