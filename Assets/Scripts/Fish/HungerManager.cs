using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HungerManager : MonoBehaviour
{
    // Singleton instance
    public static HungerManager Instance { get; private set; }

    [SerializeField] private int maxHunger = 100;
    [SerializeField] private int wellFedThreshold = 70;

    private int hungryStatus = 0;

    [SerializeField] private Slider loadBar;
    private GameObject loadPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (loadBar == null)
            loadBar = GetComponent<Slider>();

        if (loadBar != null)
        {
            loadBar.maxValue = maxHunger;
            loadBar.minValue = 0;
            loadBar.value = hungryStatus;
        }
    }

    void Update()
    {
        // Mantener el slider sincronizado en tiempo real
        if (loadBar != null && loadBar.value != hungryStatus)
            loadBar.value = hungryStatus;
    }

    // Método público principal para interactuar desde otros scripts.
    // Uso: HungerManager.Instance.Add(1);
    public void Add(int amount = 1)
    {
        if (amount <= 0) return;
        hungryStatus = Mathf.Clamp(hungryStatus + amount, 0, maxHunger);
        
        if (loadBar != null)
            loadBar.value = hungryStatus;
    }

    // Consultas de estado (opcionales)
    public int GetHungryStatus() => hungryStatus;
    public bool IsWellFed() => hungryStatus >= wellFedThreshold;

    // Reset opcional
    public void ResetHunger()
    {
        hungryStatus = 0;
        if (loadBar != null)
            loadBar.value = hungryStatus;
    }
}
