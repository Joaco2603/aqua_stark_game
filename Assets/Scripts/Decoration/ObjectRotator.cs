using UnityEngine;
using UnityEngine.InputSystem;

namespace Decoration
{
    public class ObjectRotator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 0.8f;
        [SerializeField] private bool invertX = false;
        [SerializeField] private bool invertY = false;

        private Renderer _targetRenderer;

        private void Start()
        {
            // Try to find a renderer to get the geometric center
            _targetRenderer = GetComponentInChildren<Renderer>();
        }

        private void Update()
        {
            // Ensure mouse is present
            if (Mouse.current == null) return;

            // Check if left mouse button is held down
            if (Mouse.current.leftButton.isPressed)
            {
                // Get the mouse movement delta
                Vector2 delta = Mouse.current.delta.ReadValue();

                // Calculate rotation amounts
                float rotX = delta.x * rotationSpeed * (invertX ? 1 : -1);
                float rotY = delta.y * rotationSpeed * (invertY ? -1 : 1);

                // Determine the center point for rotation
                // If a renderer is found, use its bounds center (geometric center)
                // Otherwise fallback to the transform position (pivot)
                Vector3 center = _targetRenderer != null ? _targetRenderer.bounds.center : transform.position;

                // Apply rotation
                // Rotate around World Up for horizontal mouse movement (Yaw)
                transform.RotateAround(center, Vector3.up, rotX);

                // Rotate around World Right for vertical mouse movement (Pitch)
                transform.RotateAround(center, Vector3.right, rotY);
            }
        }
    }
}
