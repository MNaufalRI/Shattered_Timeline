using UnityEngine;

public class FloatingItem_2 : MonoBehaviour
{
    [Header("Visual Settings")]
    public float rotateSpeed = 100f;
    public float floatAmplitude = 0.2f;
    public float floatFrequency = 1f;

    [Header("Pickup Settings")]
    public float pickupDelay = 1.5f;

    private float spawnTime;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        spawnTime = Time.time;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        Vector3 tempPos = startPos;
        tempPos.y += Mathf.Sin(Time.time * Mathf.PI * floatFrequency) * floatAmplitude;
        transform.position = tempPos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time >= spawnTime + pickupDelay)
            {
                AmbilItem();
            }
            else
            {
                Debug.Log("Sabar, item baru saja muncul!");
            }
        }
    }

    void AmbilItem()
    {
        Debug.Log("Item Cube berhasil masuk kantong!");
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == gameObject)
        {
            UnityEditor.Selection.activeGameObject = null;
        }
#endif
        Destroy(gameObject);
    }
}