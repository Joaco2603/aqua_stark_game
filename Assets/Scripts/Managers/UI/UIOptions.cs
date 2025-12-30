using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Singleton manager to hold UI option names and instantiate corresponding UI prefabs by name.
// Note: For Resources.Load to work, put your UI prefab assets under a "Resources/UI" folder in the project.
public class UIOptions : MonoBehaviour
{
    public static UIOptions Instance { get; private set; }

    // List of option names (you can store any data you want in the ArrayList)
    public ArrayList options = new ArrayList();

    // Optional parent where instantiated UI will be placed (set in inspector)
    public Transform uiParent;

    // Cache of loaded UI prefabs by name (loaded from Resources/UI)
    private Dictionary<string, GameObject> uiPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Simple singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Optionally preload all prefabs in Resources/UI
        PreloadUIPrefabs();
    }

    // Preloads all GameObject prefabs located in Resources/UI into the cache
    public void PreloadUIPrefabs()
    {
        uiPrefabs.Clear();
        GameObject[] loaded = Resources.LoadAll<GameObject>("UI");
        foreach (var go in loaded)
        {
            if (go != null && !uiPrefabs.ContainsKey(go.name))
                uiPrefabs.Add(go.name, go);
        }
    }

    // Add an option name to the ArrayList (avoids duplicates)
    public void AddOption(string optionName)
    {
        if (string.IsNullOrEmpty(optionName))
            return;

        if (!options.Contains(optionName))
            options.Add(optionName);
    }

    // Remove an option by name
    public void RemoveOption(string optionName)
    {
        if (options.Contains(optionName))
            options.Remove(optionName);
    }

    // Get a copy of the current options as a string array
    public string[] GetOptions()
    {
        string[] arr = new string[options.Count];
        for (int i = 0; i < options.Count; i++)
            arr[i] = options[i]?.ToString();
        return arr;
    }

    // Generate (instantiate) a UI prefab by its name. Returns the instantiated GameObject or null if not found.
    public GameObject GenerateUI(string optionName)
    {
        if (string.IsNullOrEmpty(optionName))
            return null;

        GameObject prefab = null;
        if (!uiPrefabs.TryGetValue(optionName, out prefab))
        {
            // Try to load on demand from Resources/UI
            prefab = Resources.Load<GameObject>("UI/" + optionName);
            if (prefab != null)
                uiPrefabs[optionName] = prefab;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"UIOptions: prefab named '{optionName}' not found in Resources/UI");
            return null;
        }

        GameObject instance = Instantiate(prefab, uiParent != null ? uiParent : null);
        instance.name = prefab.name; // remove (Clone) if present
        return instance;
    }

    // Clear all instantiated children under the uiParent
    public void ClearUI()
    {
        if (uiParent == null)
            return;

        for (int i = uiParent.childCount - 1; i >= 0; i--)
        {
            Destroy(uiParent.GetChild(i).gameObject);
        }
    }

    // Example helper that generates UI for every option in the list
    public void GenerateAllOptionsUI()
    {
        ClearUI();
        foreach (var obj in options)
        {
            string name = obj?.ToString();
            if (!string.IsNullOrEmpty(name))
                GenerateUI(name);
        }
    }

    // ... You can add more helpers to bind actions to created UI elements, pass data, etc.
}
