using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Fish.Entities;

public class FishPopup : MonoBehaviour
{
    public GameObject canvasTarget;
    public TextMeshProUGUI nameText;
    public Slider experienceSlider;
    public Slider hungerSlider;
    public TextMeshProUGUI expValueText;
    public TextMeshProUGUI hungerValueText;
    public FishEntity fishEntity;

    private Camera _mainCamera;

    void Awake()
    {
        if (experienceSlider != null) experienceSlider.onValueChanged.AddListener(OnExpChanged);
        if (hungerSlider != null) hungerSlider.onValueChanged.AddListener(OnHungerChanged);
    }

    void Update()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_mainCamera == null) return;

        // Check if the primary pointer (mouse/touch) was pressed this frame
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;

        // Create a ray from the camera through the pointer position
        Ray ray = _mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // If we hit THIS GameObject, trigger the interaction
            if (hit.transform == transform)
            {
                ShowPopup();
                UpdateTexts();
            }
        }
    }

    void ShowPopup()
    {
        canvasTarget.GetComponent<Menu>().Open();
    }

    void OnExpChanged(float val)
    {
        if (fishEntity != null) fishEntity.experience = Mathf.RoundToInt(val);
        UpdateTexts();
    }

    void OnHungerChanged(float val)
    {
        if (fishEntity != null) fishEntity.hunger = Mathf.RoundToInt(val);
        UpdateTexts();
    }

    void UpdateTexts()
    {
        if (fishEntity == null) return;

        if (nameText != null) nameText.text = fishEntity.fishName;

        if (experienceSlider != null)
        {
            experienceSlider.value = fishEntity.experience;
        }

        if (hungerSlider != null)
        {
            hungerSlider.value = fishEntity.hunger;
        }

        if (expValueText != null) expValueText.text = fishEntity.experience.ToString();
        if (hungerValueText != null) hungerValueText.text = fishEntity.hunger.ToString();
    }
}
