using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Adaptador para sincronizar decoraciones del servidor con el inventario local
/// Conecta tu APIAdapter con el sistema de decoraciones
/// </summary>
public class DecorationAPIAdapter : MonoBehaviour
{
    [SerializeField] private DecorationInventory decorationInventory;
    [SerializeField] private APIAdapter apiAdapter; // Tu API adapter existente
    
    void Start()
    {
        if (decorationInventory == null)
            decorationInventory = FindObjectOfType<DecorationInventory>();
        
        if (apiAdapter == null)
            apiAdapter = FindObjectOfType<APIAdapter>();
    }
    
    /// <summary>
    /// Llama al servidor para obtener las decoraciones del usuario
    /// Este es un ejemplo - adaptarlo según tu API
    /// </summary>
    public void FetchDecorationsFromServer()
    {
        // Ejemplo de cómo llamar a tu API
        // Si tienes un método en APIAdapter, úsalo aquí
        
        // StartCoroutine(GetDecorationsCoroutine());
    }
    
    /// <summary>
    /// Cargar decoraciones desde un JSON o configuración local
    /// </summary>
    public void LoadDecorationsFromJSON(string jsonPath)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
        if (jsonFile == null)
        {
            Debug.LogError($"No se encontró archivo de configuración: {jsonPath}");
            return;
        }
        
        // Parsear y cargar decoraciones
        // DecorationConfig config = JsonUtility.FromJson<DecorationConfig>(jsonFile.text);
        // Implementar según tu estructura JSON
    }
    
    /// <summary>
    /// Guardar estado actual de decoraciones colocadas
    /// </summary>
    public void SaveDecorationStates(GameObject aquariumParent)
    {
        var decorationObjects = aquariumParent.GetComponentsInChildren<DecorationObject>();
        List<DecorationPlacementData> placements = new List<DecorationPlacementData>();
        
        foreach (var decObj in decorationObjects)
        {
            placements.Add(new DecorationPlacementData
            {
                position = decObj.transform.position,
                rotation = decObj.transform.rotation,
                scale = decObj.transform.localScale
            });
        }
        
        // Guardar a JSON o enviar al servidor
        string json = JsonUtility.ToJson(new DecorationPlacementList { placements = placements });
        PlayerPrefs.SetString("decoration_placements", json);
    }
    
    /// <summary>
    /// Cargar decoraciones colocadas anteriormente
    /// </summary>
    public void LoadDecorationStates()
    {
        if (!PlayerPrefs.HasKey("decoration_placements"))
            return;
        
        string json = PlayerPrefs.GetString("decoration_placements");
        // DecorationPlacementList placements = JsonUtility.FromJson<DecorationPlacementList>(json);
        // Restaurar decoraciones según placements
    }
}

[System.Serializable]
public class DecorationPlacementData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

[System.Serializable]
public class DecorationPlacementList
{
    public List<DecorationPlacementData> placements;
}
