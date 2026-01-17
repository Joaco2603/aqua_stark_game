using UnityEngine;
using Fish.Entities;

namespace Fish.Reproduce
{
    public class Manager : MonoBehaviour
    {
        [SerializeField] private bool isReproducingEnabled = true;
        private Reproduce reproduceScript;

        private void Start()
        {
            reproducingStatusChanged();
            reproduceScript = GetComponent<Reproduce>();
        }

        private void reproducingStatusChanged(bool enabled)
        {
            GetComponent<FindAllFishes>()?.gameObject.SetActive(enabled);
            isReproducingEnabled = enabled;
        }

        /// <summary>
        /// Habilita o deshabilita la reproducción de peces
        /// </summary>
        public void SetReproducingEnabled(bool enabled)
        {
            isReproducingEnabled = enabled;
        }

        /// <summary>
        /// Obtiene el estado actual de reproducción
        /// </summary>
        public bool IsReproducingEnabled()
        {
            return isReproducingEnabled;
        }

        /// <summary>
        /// Intenta reproducir un pez solo si la reproducción está habilitada
        /// </summary>
        public FishEntity TryCreateFish(int id, string name, int experience, float hunger = 0f, GameObject prefab = null)
        {
            if (!isReproducingEnabled)
            {
                Debug.LogWarning("La reproducción está deshabilitada. No se puede crear un nuevo pez.");
                return null;
            }

            if (reproduceScript != null)
            {
                return reproduceScript.CreateFish(id, name, experience, hunger, prefab);
            }

            Debug.LogError("El script Reproduce no está disponible.");
            return null;
        }
    }
}