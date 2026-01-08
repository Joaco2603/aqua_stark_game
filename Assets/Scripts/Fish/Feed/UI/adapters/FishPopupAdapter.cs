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

    public void OnExpChanged(float val)
    {
        if (currentFish != null) currentFish.experience = Mathf.RoundToInt(val);
        UpdateTexts();
    }

    public void OnHungerChanged(float val)
    {
        if (currentFish != null) currentFish.hunger = Mathf.RoundToInt(val);
        UpdateTexts();
    }

    public void UpdateTexts()
    {
        if (currentFish == null) return;
        if (expValueText != null) expValueText.text = currentFish.experience.ToString();
        if (hungerValueText != null) hungerValueText.text = currentFish.hunger.ToString();
    }

    public void OnCloseClicked()
    {
        // Iterate all Menu components in the scene and close/destroy matching ones
        var menus = FindObjectsByType<Menu>(FindObjectsSortMode.None);
        GameObject padre = GameObject.Find("Canvas");
        GameObject principalMenu = padre.transform.Find("StartGame").gameObject;

        if (principalMenu == null)
        {
            // Fallback to find inactive object
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name == "Menu" && obj.scene.IsValid())
                {
                    principalMenu = obj;
                    break;
                }
            }
        }

        foreach (var m in menus)
        {
            if (m.gameObject != null && m.gameObject.name.Contains("FishData"))
            {
                m.Close();
                Destroy(m.gameObject);
                
                if (principalMenu != null)
                {
                    principalMenu.SetActive(true);
                    foreach (Transform childTransform in principalMenu.transform)
                    {
                        if(childTransform.name == "start")
                        {
                            continue;
                        }else{
                            var menuScript = childTransform.GetComponent<Menu>();
                            menuScript.Open();
                        }
                    }
                }
            }
        }
    }
}
