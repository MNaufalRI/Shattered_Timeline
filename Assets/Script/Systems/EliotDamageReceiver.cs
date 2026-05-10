using UnityEngine;
using Eliot.AgentComponents;
using Eliot.Environment;

public class EliotDamageReceiver : MonoBehaviour, IDamageable
{
    private AgentResources _resources;
    private Unit _unit;

    void Awake()
    {
        _resources = GetComponent<AgentResources>();
        _unit = GetComponent<Unit>();
    }

    public void TakeDamage(float amount)
    {
        // Cek tim agar tidak memukul kawan sendiri
        if (_unit != null && _unit.Team == "Player") return;

        if (_resources != null)
        {
            // Kirim aksi Reduce ke resource "Health" Eliot
            _resources.Action(new ResourceAction("Health", ResourceAffectionWay.Reduce, Mathf.RoundToInt(amount)));
        }
    }
}