using UnityEngine;

namespace UI
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] public float timeRemaining = 60f; // Configurable desde el Inspector
        [SerializeField] private Behaviour targetComponent; // Componente a desactivar o destruir
        [SerializeField] private bool destroyComponentInstead = false; // Si true, destruye en lugar de deshabilitar
        [SerializeField] private bool disableWholeGameObject = false; // Si true, desactiva todo el GameObject
        [SerializeField] private bool autoStart = true; // Iniciar automáticamente al cargar

        private bool timerIsRunning = false;

        private void Start()
        {
            if (autoStart)
            {
                StartTimer();
            }
        }

        private void Update()
        {
            if (timerIsRunning)
            {
                if (timeRemaining > 0)
                {
                    timeRemaining -= Time.deltaTime;
                }
                else
                {
                    timeRemaining = 0;
                    timerIsRunning = false;
                    OnTimerEnd();
                }
            }
        }

        public void StartTimer()
        {
            timerIsRunning = true;
        }

        public void StopTimer()
        {
            timerIsRunning = false;
        }

        private void OnTimerEnd()
        {
            // Acción al finalizar: desactivar componente, destruirlo o desactivar todo el GO
            if (disableWholeGameObject)
            {
                gameObject.SetActive(false);
                return;
            }

            if (targetComponent != null)
            {
                if (destroyComponentInstead)
                {
                    Destroy(targetComponent);
                }
                else
                {
                    targetComponent.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("Timer terminó pero no hay componente asignado para desactivar/destruir.", this);
            }
        }
    }
}