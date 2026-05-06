using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets; 

public class TargetDetectionControl : MonoBehaviour
{
    public static TargetDetectionControl instance;

    [Header("Components")]
    public PlayerControl playerControl;
    [SerializeField] private StarterAssetsInputs input; 

    [Header("Scene")]
    public List<Transform> allTargetsInScene = new List<Transform>();

    [Header("Target Detection")]
    public LayerMask whatIsEnemy;
    public bool canChangeTarget = true;

    [Range(0f, 15f)] public float detectionRange = 10f;
    [Range(0f, 1f)] public float dotProductThreshold = 0.15f;

    [Header("Debug")]
    public bool debug;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        PopulateTargetInScene();
        StartCoroutine(RunEveryXms());

        if (input == null)
            input = GetComponent<StarterAssetsInputs>();
    }

    private void PopulateTargetInScene()
    {
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();

        foreach (EnemyBase obj in enemies)
        {
            allTargetsInScene.Add(obj.transform);
        }

        if (debug)
            Debug.Log("Targets found: " + allTargetsInScene.Count);
    }

    private IEnumerator RunEveryXms()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            GetEnemyInInputDirection();
        }
    }

    public void GetEnemyInInputDirection()
    {
        if (!canChangeTarget) return;

        Vector3 inputDirection = new Vector3(input.move.x, 0, input.move.y);

        if (inputDirection.sqrMagnitude < 0.01f)
            return;

        inputDirection = Camera.main.transform.TransformDirection(inputDirection);
        inputDirection.y = 0;
        inputDirection.Normalize();

        Transform closestEnemy = GetClosestEnemyInDirection(inputDirection);

        if (closestEnemy != null &&
            Vector3.Distance(transform.position, closestEnemy.position) <= detectionRange)
        {
            playerControl.ChangeTarget(closestEnemy);

            if (debug)
                Debug.Log("Target: " + closestEnemy.name);
        }
    }

    Transform GetClosestEnemyInDirection(Vector3 inputDirection)
    {
        Transform closestEnemy = null;
        float maxDot = dotProductThreshold;

        foreach (Transform enemy in allTargetsInScene)
        {
            if (enemy == null) continue;

            Vector3 dir = (enemy.position - transform.position).normalized;
            float dot = Vector3.Dot(inputDirection, dir);

            if (dot > maxDot)
            {
                maxDot = dot;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }
}