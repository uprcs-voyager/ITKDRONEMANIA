using UnityEngine;
using UnityEngine.EventSystems;

public class DroneModelRotator : MonoBehaviour, IDragHandler
{
    [SerializeField] private Transform droneModelTransform;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private bool autoRotate = true;
    [SerializeField] private float autoRotateSpeed = 15f;

    private void Update()
    {
        // Rotasi otomatis lambat saat tidak di-drag oleh pemain
        if (autoRotate && droneModelTransform != null)
        {
            droneModelTransform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (droneModelTransform == null) return;

        // Putar drone berdasarkan geseran horizontal & vertikal mouse/touch
        float rotX = eventData.delta.x * rotationSpeed;
        float rotY = eventData.delta.y * rotationSpeed;

        droneModelTransform.Rotate(Vector3.up, -rotX, Space.World);
        droneModelTransform.Rotate(Vector3.right, rotY, Space.World);
    }
}