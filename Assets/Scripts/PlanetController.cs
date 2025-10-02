using UnityEngine;
using UnityEngine.InputSystem; // Required for new Input System

public class PlanetController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 20f;   // Rotation smoothness
    [SerializeField] private Vector3 rotationOffset;     // Offset to tweak rotation

    void Update()
    {
        // Rotation
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, Camera.main.WorldToScreenPoint(transform.position).z)
        );

        Vector3 direction = worldMousePos - transform.position;
        direction.z = 0; // keep 2D rotation
        Debug.DrawLine(transform.position, transform.position + direction);

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
            targetRotation *= Quaternion.Euler(rotationOffset);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
