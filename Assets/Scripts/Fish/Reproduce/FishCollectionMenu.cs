using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Fish.Entities;

namespace Fish.Reproduce
{
    public class FishCollectionMenu : MonoBehaviour
    {
        private FindAllFishes fishFinder;
        private Reproduce reproducer;

        [Header("UI Elements")]
        [SerializeField] private Transform fishListContainer;
        [SerializeField] private GameObject fishButtonPrefab;
        [SerializeField] private Button reproduceButton;

        [Header("Selection Settings")]
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color normalColor = Color.white;
        
        private List<FishEntity> collectedFishes = new List<FishEntity>();
        private List<FishEntity> selectedParents = new List<FishEntity>();
        private Dictionary<FishEntity, Image> fishButtonImages = new Dictionary<FishEntity, Image>();
        
        private void Start()
        {
            fishFinder = GetComponent<FindAllFishes>();
            reproducer = GetComponent<Reproduce>();
            
            if (fishFinder == null)
                Debug.LogError("FindAllFishes component not found on this GameObject.");
                
            if (reproducer == null)
                Debug.LogError("Reproduce component not found on this GameObject.");

            if (reproduceButton != null)
            {
                reproduceButton.onClick.AddListener(OnReproduceClicked);
                reproduceButton.interactable = false;
            }
        }

        
        /// <summary>
        /// Actualiza la lista de peces conseguidos
        /// </summary>
        public void RefreshCollection()
        {
            if (fishFinder == null)
            {
                Debug.LogError("FindAllFishes component not available.");
                return;
            }

            // Get all fishes
            GameObject[] allFishes = fishFinder.FindAll();
            
            Debug.Log($"Fishes found: {allFishes.Length}");

            // Clear previous lists
            collectedFishes.Clear();
            fishButtonImages.Clear();
            selectedParents.Clear();
            UpdateReproduceButtonState();
            
            ClearMenuDisplay();

            // Process each fish found
            foreach (GameObject fish in allFishes)
            {
                FishEntity fishEntity = fish.GetComponent<FishEntity>();
                if (fishEntity != null)
                {
                    collectedFishes.Add(fishEntity);
                    DisplayFishItem(fishEntity);
                }
            }
        }

        /// <summary>
        /// Muestra un pez en el menú
        /// </summary>
        private void DisplayFishItem(FishEntity fish)
        {
            if (fishButtonPrefab == null || fishListContainer == null)
            {
                Debug.LogWarning("FishButtonPrefab o FishListContainer no están asignados.");
                return;
            }

            GameObject itemUI = Instantiate(fishButtonPrefab, fishListContainer);
            
            // Configurar botón
            Button btn = itemUI.GetComponent<Button>();
            Image btnImage = itemUI.GetComponent<Image>();
            
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnFishSelected(fish));
            }
            
            if (btnImage != null)
            {
                btnImage.color = normalColor;
                fishButtonImages[fish] = btnImage;
            }

            // Mostrar nombre del pez en el botón
            Text nameText = itemUI.GetComponentInChildren<Text>();
            if (nameText != null)
            {
                nameText.text = fish.name;
            }

            // Buscar componentes de texto para mostrar información adicional
            Text[] textComponents = itemUI.GetComponentsInChildren<Text>();
            if (textComponents.Length > 1) textComponents[1].text = $"ID: {fish.id}";
            if (textComponents.Length > 2) textComponents[2].text = $"Exp: {fish.experience}";
            if (textComponents.Length > 3) textComponents[3].text = $"Hambre: {fish.hunger:F1}";
        }
        private void OnFishSelected(FishEntity fish)
        {
            if (selectedParents.Contains(fish))
            {
                selectedParents.Remove(fish);
                SetFishButtonColor(fish, normalColor);
            }
            else
            {
                if (selectedParents.Count < 2)
                {
                    selectedParents.Add(fish);
                    SetFishButtonColor(fish, selectedColor);
                }
            }
            UpdateReproduceButtonState();
        }

        private void SetFishButtonColor(FishEntity fish, Color color)
        {
            if (fishButtonImages.ContainsKey(fish))
            {
                fishButtonImages[fish].color = color;
            }
        }

        private void UpdateReproduceButtonState()
        {
            if (reproduceButton != null)
            {
                reproduceButton.interactable = (selectedParents.Count == 2);
            }
        }

        private void OnReproduceClicked()
        {
            if (selectedParents.Count != 2) return;

            if (reproducer != null)
            {
                int newId = Random.Range(1000, 9999);
                string newName = "New Baby Fish";
                FishEntity child = reproducer.ReproduceEntities(selectedParents.ToArray(), newId, newName);
                if (child != null)
                {
                    Debug.Log("Reproducción exitosa!");
                    fishListContainer.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Limpia los elementos del menú
        /// </summary>
        private void ClearMenuDisplay()
        {
            if (fishListContainer != null)
            {
                foreach (Transform child in fishListContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Obtiene la cantidad total de peces conseguidos
        /// </summary>
        public int GetCollectionCount()
        {
            return collectedFishes.Count;
        }

        /// <summary>
        /// Obtiene un pez de la colección por índice
        /// </summary>
        public FishEntity GetCollectedFish(int index)
        {
            if (index >= 0 && index < collectedFishes.Count)
            {
                return collectedFishes[index];
            }
            return null;
        }

        /// <summary>
        /// Obtiene todos los peces de la colección
        /// </summary>
        public List<FishEntity> GetAllCollectedFishes()
        {
            return new List<FishEntity>(collectedFishes);
        }

    }
}
