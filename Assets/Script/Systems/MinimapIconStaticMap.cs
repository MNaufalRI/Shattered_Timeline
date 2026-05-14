using UnityEngine;

public class MinimapIconStaticMap : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Tarik objek Player utama ke sini")]
    public Transform playerTransform;

    [Header("Height Settings")]
    [Tooltip("Ketinggian ikon agar selalu berada di atas objek lain di minimap")]
    public float iconHeight = 10f;

    [Header("Rotation Offset")]
    public bool lockRotationX = true;
    public bool lockRotationZ = true;

    void LateUpdate()
    {
        if (playerTransform == null) return;
        Vector3 newPosition = playerTransform.position;
        newPosition.y = iconHeight;
        transform.position = newPosition;

        Vector3 playerRotation = playerTransform.eulerAngles;

        float rotX = lockRotationX ? 90f : transform.eulerAngles.x; 
        float rotZ = lockRotationZ ? 0f : transform.eulerAngles.z;

        transform.rotation = Quaternion.Euler(rotX, playerRotation.y, rotZ);
    }
}