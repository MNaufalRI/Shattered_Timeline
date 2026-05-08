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

        // StartCoroutine(RunEveryXms()); // <-- Dihapus agar tidak mendeteksi secara otomatis

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

    // Fungsi ini diganti namanya dan sekarang hanya dieksekusi jika dipanggil dari script lain (saat klik kiri)
    public void LockOnToTarget()
    {
        if (!canChangeTarget) return;

        Vector3 direction;

        // Jika player sedang memberikan input pergerakan (WASD/Analog)
        if (input != null && new Vector2(input.move.x, input.move.y).sqrMagnitude > 0.01f)
        {
            direction = new Vector3(input.move.x, 0, input.move.y);
            direction = Camera.main.transform.TransformDirection(direction);
        }
        else
        {
            // Fallback: Jika diam di tempat, cari musuh sesuai arah hadap karakter
            direction = transform.forward;
        }

        direction.y = 0;
        direction.Normalize();

        Transform closestEnemy = GetClosestEnemyInDirection(direction);

        if (closestEnemy != null && Vector3.Distance(transform.position, closestEnemy.position) <= detectionRange)
        {
            playerControl.ChangeTarget(closestEnemy);

            if (debug)
                Debug.Log("Target Locked: " + closestEnemy.name);
        }
    }

    Transform GetClosestEnemyInDirection(Vector3 direction)
    {
        Transform closestEnemy = null;
        float maxDot = dotProductThreshold;

        foreach (Transform enemy in allTargetsInScene)
        {
            if (enemy == null) continue;

            Vector3 dir = (enemy.position - transform.position).normalized;
            float dot = Vector3.Dot(direction, dir);

            if (dot > maxDot)
            {
                maxDot = dot;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }
}