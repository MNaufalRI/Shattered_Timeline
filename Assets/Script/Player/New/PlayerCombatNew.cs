using UnityEngine;
using DG.Tweening;
using StarterAssets;

public class PlayerControl : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerMovement2 thirdPersonController;

    [Header("Combat")]
    public Transform target;
    [SerializeField] private Transform attackPos;
    [SerializeField] private float quickAttackDeltaDistance;
    [SerializeField] private float heavyAttackDeltaDistance;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float airknockbackForce = 10f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float reachTime = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    private bool isAttacking = false;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack(0); // klik kiri
        }

        if (Input.GetMouseButtonDown(1))
        {
            Attack(1); // klik kanan
        }

        // optional keyboard
        if (Input.GetKeyDown(KeyCode.J))
        {
            Attack(0);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Attack(1);
        }
    }

    public void Attack(int attackState)
    {
        if (isAttacking) return;

        thirdPersonController.canMove = false;

        RandomAttackAnim(attackState);
    }

    void RandomAttackAnim(int attackState)
    {
        if (attackState == 0)
            QuickAttack();
        else
            HeavyAttack();
    }

    public void GetClose()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;
        FaceThis(targetPos);

        Vector3 finalPos = Vector3.MoveTowards(targetPos, transform.position, 1.4f);

        finalPos.y = transform.position.y;

        transform.DOMove(finalPos, 0.2f);
    }

    void QuickAttack()
    {
        if (target == null)
        {
            thirdPersonController.canMove = true;
            return;
        }

        int attackIndex = Random.Range(1, 4);

        switch (attackIndex)
        {
            case 1:
                MoveTowardsTarget(target.position, quickAttackDeltaDistance, "punch");
                break;
            case 2:
                MoveTowardsTarget(target.position, quickAttackDeltaDistance, "kick");
                break;
            case 3:
                MoveTowardsTarget(target.position, quickAttackDeltaDistance, "mmakick");
                break;
        }

        isAttacking = true;
    }

    void HeavyAttack()
    {
        if (target == null)
        {
            thirdPersonController.canMove = true;
            return;
        }

        int attackIndex = Random.Range(1, 3);

        FaceThis(target.position);

        if (attackIndex == 1)
            anim.SetBool("heavyAttack1", true);
        else
            anim.SetBool("heavyAttack2", true);

        isAttacking = true;
    }

    public void ResetAttack()
    {
        anim.SetBool("punch", false);
        anim.SetBool("kick", false);
        anim.SetBool("mmakick", false);
        anim.SetBool("heavyAttack1", false);
        anim.SetBool("heavyAttack2", false);

        thirdPersonController.canMove = true;
        isAttacking = false;
    }

    public void PerformAttack()
    {
        Collider[] enemies = Physics.OverlapSphere(
            attackPos.position,
            attackRange,
            enemyLayer
        );

        foreach (var enemy in enemies)
        {
            EnemySimple enemySimple = enemy.GetComponent<EnemySimple>();
            if (enemySimple != null)
            {
                enemySimple.TakeDamage(10f);
            }

            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.OnHit();
            }
        }
    }

    public void MoveTowardsTarget(Vector3 target_, float deltaDistance, string animName)
    {
        anim.SetBool(animName, true);

        FaceThis(target_);

        Vector3 finalPos = Vector3.MoveTowards(target_, transform.position, deltaDistance);
        finalPos.y = transform.position.y;

        transform.DOMove(finalPos, reachTime);
    }

    public void FaceThis(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;

        if (dir == Vector3.zero) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.DORotateQuaternion(rot, 0.2f);
    }

    public void ChangeTarget(Transform newTarget)
    {
        target = newTarget;

        EnemyBase enemy = newTarget.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            Debug.Log("Target locked: " + enemy.name);
        }
    }
}