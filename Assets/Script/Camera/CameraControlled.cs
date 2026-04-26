using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset; 
    public float zoomSensitivity = 0.01f;

    float currentZoom = 1f;
    float targetZoom = 1f;

    void LateUpdate()
    {
        if (target == null) return;

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y * zoomSensitivity;
            targetZoom = Mathf.Clamp(targetZoom - scroll, 0.3f, 3f);
        }
        currentZoom = Mathf.MoveTowards(currentZoom, targetZoom, Time.deltaTime * 5f);

 
        transform.position = target.position + offset * currentZoom;

        transform.LookAt(target.position);

    }
}