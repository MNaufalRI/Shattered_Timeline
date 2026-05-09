using UnityEngine;
using DG.Tweening;

public class GluttonyMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator anim; 

    [Header("Targeting")]
    public Transform player;

    [Header("Speed Settings")]
    public float baseSpeed = 3.5f;
    public float currentSpeed;
    public float rotationSpeed = 8f;

    [Header("Stopping Distance")]
    public float stopDistance = 2f;

    [HideInInspector] public bool canMove = true;

    void Start()
    {
        currentSpeed = baseSpeed;

        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null || !canMove)
        {
            UpdateAnimation(0);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        if (distance > stopDistance)
        {

            Vector3 targetPos = player.position - (dir * stopDistance);
            targetPos.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

 
            UpdateAnimation(currentSpeed);
        }
        else
        {
            UpdateAnimation(0);
        }
    }

    public void FacePlayerSmooth(float duration = 0.2f)
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
 
            Quaternion targetRot = Quaternion.LookRotation(dir);

            transform.DORotateQuaternion(targetRot, duration);
        }
    }

    private void UpdateAnimation(float speed)
    {
        if (anim != null)
        {
            anim.SetFloat("Speed", speed);
        }
    }

    public void ApplySpeedBuff(float multiplier, float duration)
    {
        StartCoroutine(SpeedBuffRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator SpeedBuffRoutine(float multiplier, float duration)
    {
        currentSpeed = baseSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        currentSpeed = baseSpeed;
    }
}