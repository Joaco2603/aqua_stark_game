using UnityEngine;
using UnityEngine.InputSystem;

namespace Decoration
{
    public class Placement : MonoBehaviour
    {
        private GameObject _currentPlacingObject;
        private bool _justStartedPlacing;
        private System.Action _onPlacementFinished;
        private Transform _target;

        public bool IsPlacing => _currentPlacingObject != null;

        public void StartPlacing(GameObject prefab, Transform target, System.Action onFinished = null)
        {
            if (_currentPlacingObject != null)
            {
                Destroy(_currentPlacingObject);
            }

            _onPlacementFinished = onFinished;
            _target = target;

            if (prefab != null)
            {
                _currentPlacingObject = Instantiate(prefab);
                _justStartedPlacing = true;
            }
        }

        private void LateUpdate()
        {
            if (!IsPlacing) return;
            
            HandlePlacement();
        }

        private void HandlePlacement()
        {
            if (_target == null) return;
            if (Mouse.current == null) return;

            // Raycast desde la cámara
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            // Plano que mira a la cámara pasando por el target
            Plane plane = new Plane(-transform.forward, _target.position);
            
            float enter;
            if (plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                // Fijar Y arriba del target, mantener Z del target, variar X
                Vector3 newPos = new Vector3(hitPoint.x, _target.position.y, _target.position.z);
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
    }
}