using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class FishData : MonoBehaviour
{
    [Header("Fish Data")]
    [Tooltip("Visible name for the fish")]
    public string fishName = "Fish";

    [Tooltip("Default experience points for this fish (editable in Inspector)")]
    public int experience = 10;

    [Tooltip("Default hunger value for this fish (editable in Inspector)")]
    public int hunger = 5;

    [Header("UI")]
    [Tooltip("Assign a Menu prefab (must contain a Menu component). This prefab should also include a FishPopup component to receive the data.")]
    public Menu popupMenuPrefab;

    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;

        // Ensure sensible defaults if Unity left them at zero or negative
        if (experience <= 0) experience = 10;
        if (hunger <= 0) hunger = 5;
    }

    // Check for input every frame (New Input System replacement for OnMouseDown)
    void Update()
    {
        // Check if the primary pointer (mouse/touch) was pressed this frame
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            // Create a ray from the camera through the pointer position
            Ray ray = _mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());
            
            // Perform the raycast
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // If we hit THIS GameObject, trigger the interaction
                if (hit.transform == transform)
                {
                    Interact();
                }
            }
        }
    }

    void Interact()
    {
        if (popupMenuPrefab != null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            GameObject go;
            if (canvas != null)
                go = Instantiate(popupMenuPrefab.gameObject, canvas.transform);
            else
                go = Instantiate(popupMenuPrefab.gameObject);

            Menu menuInstance = go.GetComponent<Menu>();
            if (menuInstance == null)
            {
                Debug.LogWarning("FishData: popupMenuPrefab does not contain a Menu component.");
                return;
            }

            // Try to call a strongly-typed receiver if present (keeps compatibility with FishPopup)
            // Prefer a type-safe receiver (mapper) if present
            var receiver = menuInstance.GetComponent<IFishPopupReceiver>();
            if (receiver != null)
            {
                receiver.SetData(this);
            }
            else
            {
                // Backwards-compatible: try existing FishPopup component
                var fishPopup = menuInstance.GetComponent<FishPopup>();
                if (fishPopup != null) fishPopup.SetData(this);
                else
                {
                    // Final fallback: SendMessage to support custom prefabs without the adapter
                    menuInstance.gameObject.SendMessage("SetData", this, SendMessageOptions.DontRequireReceiver);
                }
            }

            var mm = FindFirstObjectByType<MenuManager>();
            if (mm != null) mm.OpenMenu(menuInstance);
            else menuInstance.Open();
            return;
        }

        // If no prefab assigned, try to find an existing FishPopup in scene
        FishPopup existing = FindFirstObjectByType<FishPopup>();
        if (existing != null)
        {
            existing.SetData(this);
            Menu menu = existing.GetComponent<Menu>();
            var mm = FindFirstObjectByType<MenuManager>();
            if (mm != null && menu != null) mm.OpenMenu(menu);
            else if (menu != null) menu.Open();
            return;
        }
    }
}
