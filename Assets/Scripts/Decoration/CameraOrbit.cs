using UnityEngine;
using UnityEngine.InputSystem;

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

        private float _currentX = 0.0f;
        private float _currentY = 0.0f;
        private bool _isMoveMode = false;

        // Decoration Placement
        private GameObject _currentPlacingObject;
        private bool _justStartedPlacing;
        private System.Action _onPlacementFinished;

        public void StartPlacing(GameObject prefab, System.Action onFinished = null)
        {
            if (_currentPlacingObject != null)
            {
                Destroy(_currentPlacingObject);
            }

            _onPlacementFinished = onFinished;

            if (prefab != null)
            {
                _currentPlacingObject = Instantiate(prefab);
                _justStartedPlacing = true;
            }
        }

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

            if (_currentPlacingObject != null)
            {
                HandlePlacement();
            }
            // Solo procesar si se mantiene presionado el botón izquierdo del mouse
            else if (Mouse.current.leftButton.isPressed)
            {
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

        private void HandlePlacement()
        {
            // Raycast desde la cámara
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            // Plano que mira a la cámara pasando por el target
            Plane plane = new Plane(-transform.forward, target.position);
            
            float enter;
            if (plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                // Fijar Y arriba del target, mantener Z del target, variar X
                Vector3 newPos = new Vector3(hitPoint.x, target.position.y, target.position.z);
                _currentPlacingObject.transform.position = newPos;
            }

            // Confirmar con clic izquierdo
            if (Mouse.current.leftButton.wasPressedThisFrame && !_justStartedPlacing)
            {
                _currentPlacingObject = null;
                _onPlacementFinished?.Invoke();
                _onPlacementFinished = null;
            }

            _justStartedPlacing = false;
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F) angle += 360F;
            if (angle > 360F) angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }

        // Métodos públicos para conectar con botones UI

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
