using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("VFX & Target")]
    [SerializeField] private GameObject hitVfx;
    [SerializeField] private GameObject activeTargetObject;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        ActiveTarget(false);
    }

    public void SpawnHitVfx(Vector3 pos)
    {
        if (hitVfx != null)
        {
            Instantiate(hitVfx, pos, Quaternion.identity);
        }
    }

    public void ActiveTarget(bool value)
    {
        if (activeTargetObject != null)
        {
            activeTargetObject.SetActive(value);
        }
    }

    public void OnHit()
    {
        if (!gameObject.activeInHierarchy) return;

        StartCoroutine(HitPause());
    }

    IEnumerator HitPause()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        yield return new WaitForSeconds(0.1f);

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }
}