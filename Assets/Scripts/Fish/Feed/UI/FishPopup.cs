using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FishPopup : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Slider experienceSlider;
    public Slider hungerSlider;
    public TextMeshProUGUI expValueText;
    public TextMeshProUGUI hungerValueText;
    public Button closeButton;

    private FishData fish;

    void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        if (experienceSlider != null) experienceSlider.onValueChanged.AddListener(OnExpChanged);
        if (hungerSlider != null) hungerSlider.onValueChanged.AddListener(OnHungerChanged);
    }

    public void SetData(FishData f)
    {
        fish = f;
        if (nameText != null) nameText.text = f.fishName;

        if (experienceSlider != null)
        {
            experienceSlider.maxValue = Mathf.Max(1, f.experience * 2);
            experienceSlider.value = f.experience;
        }

        if (hungerSlider != null)
        {
            hungerSlider.maxValue = 100;
            hungerSlider.value = f.hunger;
        }

        UpdateTexts();
    }

    void OnExpChanged(float val)
    {
        if (fish != null) fish.experience = Mathf.RoundToInt(val);
        UpdateTexts();
    }

    void OnHungerChanged(float val)
    {
        if (fish != null) fish.hunger = Mathf.RoundToInt(val);
        UpdateTexts();
    }

    void UpdateTexts()
    {
        if (fish == null) return;
        if (expValueText != null) expValueText.text = fish.experience.ToString();
        if (hungerValueText != null) hungerValueText.text = fish.hunger.ToString();
    }

    void OnCloseClicked()
    {
        // If Menu component is present, let MenuManager handle closing via Back or close logic.
        var menu = GetComponent<Menu>();
        if (menu != null)
        {
            menu.Close();
            return;
        }

        // Fallback: destroy this popup GameObject
        Destroy(gameObject);
    }
}
