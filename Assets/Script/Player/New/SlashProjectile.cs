using UnityEngine;

public class SlashProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;
    public float damage = 50f;
    public LayerMask enemyLayer;
    public GameObject hitVFX;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);

                if (hitVFX != null)
                {
                    Instantiate(hitVFX, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}