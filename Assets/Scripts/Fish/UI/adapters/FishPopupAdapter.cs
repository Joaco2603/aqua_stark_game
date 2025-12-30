using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Small adapter you can add to your prefab to map FishData -> UI (implements IFishPopupReceiver)
public class FishPopupAdapter : MonoBehaviour, IFishPopupReceiver
{
    public TextMeshProUGUI nameText;
    public Slider experienceSlider;
    public Slider hungerSlider;
    public TextMeshProUGUI expValueText;
    public TextMeshProUGUI hungerValueText;
    public Button closeButton;

    private FishData currentFish;

    void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        if (experienceSlider != null) experienceSlider.onValueChanged.AddListener(OnExpChanged);
        if (hungerSlider != null) hungerSlider.onValueChanged.AddListener(OnHungerChanged);
    }

    public void SetData(FishData fish)
    {
        currentFish = fish;
        if (nameText != null) nameText.text = fish.fishName;

        if (experienceSlider != null)
        {
            experienceSlider.maxValue = Mathf.Max(1, fish.experience * 2);
            experienceSlider.value = fish.experience;
        }

        if (hungerSlider != null)
        {
            hungerSlider.maxValue = 100;
            hungerSlider.value = fish.hunger;
        }

        UpdateTexts();
    }

    void OnExpChanged(float val)
    {
        if (currentFish != null) currentFish.experience = Mathf.RoundToInt(val);
        UpdateTexts();
    }

    void OnHungerChanged(float val)
    {
        if (currentFish != null) currentFish.hunger = Mathf.RoundToInt(val);
        UpdateTexts();
    }

    void UpdateTexts()
    {
        if (currentFish == null) return;
        if (expValueText != null) expValueText.text = currentFish.experience.ToString();
        if (hungerValueText != null) hungerValueText.text = currentFish.hunger.ToString();
    }

    void OnCloseClicked()
    {
        var menu = GetComponent<Menu>();
        if (menu != null)
        {
            menu.Close();
            return;
        }

        Destroy(gameObject);
    }
}
