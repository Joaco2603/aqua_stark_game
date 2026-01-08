using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Decoration
{
    public class CameraOrbit : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("El objeto alrededor del cual orbitará la cámara (la pecera).")]
        [SerializeField] private Transform target;

        [Header("Orbit Settings")]
        [SerializeField] private float distance = 10.0f;
        [SerializeField] private float verticalOffset = 4.0f;
        [SerializeField] private float xSpeed = 0.5f;
        [SerializeField] private float ySpeed = 0.5f;
        [Tooltip("Límite inferior del ángulo vertical (en grados).")]
        [SerializeField] private float yMinLimit = 0f;
        [Tooltip("Límite superior del ángulo vertical (en grados).")]
        [SerializeField] private float yMaxLimit = 60f;

        [Header("Pan Settings")]
        [SerializeField] private float panSpeed = 0.02f;

        [Header("Interaction Gate")]
        [Tooltip("Capas que bloquean la órbita/pan cuando el cursor está sobre ellas.")]
        [SerializeField] private LayerMask blockOrbitMask = 0;

        private float _currentX = 0.0f;
        private float _currentY = 0.0f;
        private bool _isMoveMode = false;

        private void Start()
        {
            Vector3 angles = transform.eulerAngles;
            _currentX = angles.y;
            _currentY = angles.x;

            // Si no hay target, crear uno temporal o usar la posición actual
            if (target == null)
            {
                GameObject go = new GameObject("CameraTarget");
                go.transform.position = transform.position + transform.forward * distance;
                target = go.transform;
            }
            else
            {
                // Calcular distancia inicial basada en la posición actual
                distance = Vector3.Distance(transform.position, target.position);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;
            if (Mouse.current == null) return;

            // Para pan se requiere modo activo y tecla R presionada.
            bool rHeld = Keyboard.current != null && Keyboard.current.rKey.isPressed;

            // Solo procesar si se mantiene presionado el botón izquierdo del mouse
            if (Mouse.current.leftButton.isPressed && rHeld)
            {
                // Si el puntero está sobre una capa interactuable, no orbitar/panear.
                var cam = GetComponent<Camera>();
                if (cam == null) cam = Camera.main;
                if (cam != null && blockOrbitMask.value != 0)
                {
                    Vector2 screenPos = Mouse.current.position.ReadValue();
                    Ray ray = cam.ScreenPointToRay(screenPos);
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, blockOrbitMask))
                    {
                        // Bloquear control de cámara este frame para no interferir.
                        return;
                    }
                }

                Vector2 delta = Mouse.current.delta.ReadValue();

                

                if (_isMoveMode)
                {
                    // Modo Desplazamiento (Pan): Mover el target relativo a la cámara
                    Vector3 right = transform.right;
                    Vector3 up = transform.up;
                    
                    // Mover el target en dirección opuesta al mouse para efecto de "arrastre"
                    Vector3 move = (-right * delta.x * panSpeed) + (-up * delta.y * panSpeed);
                    target.position += move;
                }
                else
                {
                    // Modo Rotación (Orbit): Rotar la cámara alrededor del target
                    _currentX += delta.x * xSpeed;
                    _currentY -= delta.y * ySpeed;

                    _currentY = ClampAngle(_currentY, yMinLimit, yMaxLimit);
                }
            }
            
            // Zoom con la rueda del mouse (opcional)
            float scroll = Mouse.current.scroll.y.ReadValue();
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance -= scroll * 0.01f;
                distance = Mathf.Max(distance, 1.0f);
            }

            // Aplicar transformación
            Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + Vector3.up * verticalOffset;

            transform.rotation = rotation;
            transform.position = position;
        }



        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F) angle += 360F;
            if (angle > 360F) angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }

        // Métodos públicos para conectar con botones UI


        public Transform Target => target;

        /// <summary>
        /// Activa el modo de desplazamiento (Pan).
        /// </summary>
        public void EnableMoveMode()
        {
            _isMoveMode = true;
        }

        /// <summary>
        /// Activa el modo de rotación (Orbit).
        /// </summary>
        public void EnableRotateMode()
        {
            _isMoveMode = false;
        }

        /// <summary>
        /// Alterna entre modo rotación y desplazamiento.
        /// </summary>
        public void ToggleMode()
        {
            _isMoveMode = !_isMoveMode;
        }
    }
}
