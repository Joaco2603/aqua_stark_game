using UnityEngine;

/// <summary>
/// Ejemplos avanzados de cómo usar el sistema de decoraciones
/// Copia los métodos que necesites a tus scripts
/// </summary>
public class DecorationExamples : MonoBehaviour
{
    private DecorationInventory decorationInventory;
    private DecorationPlacer decorationPlacer;
    
    void Start()
    {
        decorationInventory = FindObjectOfType<DecorationInventory>();
        decorationPlacer = FindObjectOfType<DecorationPlacer>();
    }
    
    // ===== EJEMPLOS DE INVENTARIO =====
    
    /// <summary>
    /// Ejemplo 1: Agregar una decoración al inventario
    /// </summary>
    public void ExampleAddDecoration(GameObject prefab, Sprite icon, string name)
    {
        var decoration = new DecorationData(
            id: Random.Range(1000, 9999),
            name: name,
            description: $"Una hermosa {name}",
            icon: icon,
            prefab: prefab,
            quantity: 3
        );
        
        decorationInventory.AddDecoration(decoration);
        Debug.Log($"✅ {name} agregada al inventario");
    }
    
    /// <summary>
    /// Ejemplo 2: Obtener cantidad de decoraciones
    /// </summary>
    public void ExampleCheckInventory()
    {
        var decorations = decorationInventory.GetDecorations();
        Debug.Log($"Total de tipos de decoración: {decorations.Count}");
        
        foreach (var decoration in decorations)
        {
            Debug.Log($"{decoration.name}: {decoration.quantity} unidades");
        }
    }
    
    /// <summary>
    /// Ejemplo 3: Verificar si tienes una decoración específica
    /// </summary>
    public void ExampleHasDecoration(int id)
    {
        if (decorationInventory.HasDecoration(id))
        {
            var decoration = decorationInventory.GetDecorationById(id);
            Debug.Log($"Tienes {decoration.quantity} de {decoration.name}");
        }
        else
        {
            Debug.Log("No tienes esa decoración");
        }
    }
    
    /// <summary>
    /// Ejemplo 4: Eliminar una decoración del inventario
    /// </summary>
    public void ExampleRemoveDecoration(int id)
    {
        decorationInventory.RemoveDecoration(id);
        Debug.Log($"Decoración {id} eliminada del inventario");
    }
    
    // ===== EJEMPLOS DE COLOCACIÓN =====
    
    /// <summary>
    /// Ejemplo 5: Iniciar colocación programáticamente
    /// </summary>
    public void ExampleStartPlacingProgrammatically(int decorationId)
    {
        var decoration = decorationInventory.GetDecorationById(decorationId);
        if (decoration != null)
        {
            decorationPlacer.StartPlacing(decoration);
            Debug.Log($"Colocando {decoration.name}...");
        }
    }
    
    /// <summary>
    /// Ejemplo 6: Colocar decoración en posición específica (sin interactividad)
    /// </summary>
    public void ExamplePlaceDecorationAtPosition(int decorationId, Vector3 position)
    {
        var decoration = decorationInventory.GetDecorationById(decorationId);
        if (decoration == null || decoration.prefab == null) return;
        
        var instance = Instantiate(decoration.prefab, position, Quaternion.identity);
        instance.AddComponent<DecorationObject>();
        
        decorationInventory.RemoveDecoration(decorationId);
        Debug.Log($"Decoración colocada en {position}");
    }
    
    // ===== EJEMPLOS CON EVENTOS =====
    
    /// <summary>
    /// Ejemplo 7: Crear un manager que reaccione a eventos
    /// </summary>
    public void ExampleCreateEventSystem()
    {
        // Esto es un pseudocódigo, necesitarías implementar eventos en los scripts
        // decorationPlacer.OnDecorationPlaced += HandleDecorationPlaced;
        // decorationUI.OnItemSelected += HandleItemSelected;
    }
    
    private void HandleDecorationPlaced(DecorationData decoration)
    {
        Debug.Log($"Se colocó {decoration.name}");
        // Reproducir sonido, efecto visual, etc.
    }
    
    private void HandleItemSelected(DecorationData decoration)
    {
        Debug.Log($"Se seleccionó {decoration.name}");
    }
    
    // ===== EJEMPLOS DE GUARDADO =====
    
    /// <summary>
    /// Ejemplo 8: Guardar estado del inventario en PlayerPrefs
    /// </summary>
    public void ExampleSaveInventory()
    {
        var decorations = decorationInventory.GetDecorations();
        string json = JsonUtility.ToJson(new DecorationList { decorations = decorations });
        PlayerPrefs.SetString("decoration_inventory", json);
        Debug.Log("✅ Inventario guardado");
    }
    
    /// <summary>
    /// Ejemplo 9: Cargar inventario desde PlayerPrefs
    /// </summary>
    public void ExampleLoadInventory()
    {
        if (PlayerPrefs.HasKey("decoration_inventory"))
        {
            string json = PlayerPrefs.GetString("decoration_inventory");
            // DecorationList list = JsonUtility.FromJson<DecorationList>(json);
            Debug.Log("✅ Inventario cargado");
        }
    }
    
    // ===== EJEMPLOS DE BÚSQUEDA Y FILTRADO =====
    
    /// <summary>
    /// Ejemplo 10: Buscar decoraciones por nombre
    /// </summary>
    public void ExampleSearchByName(string searchText)
    {
        var decorations = decorationInventory.GetDecorations();
        var results = decorations.FindAll(d => d.name.ToLower().Contains(searchText.ToLower()));
        
        foreach (var decoration in results)
        {
            Debug.Log($"Encontrado: {decoration.name}");
        }
    }
    
    /// <summary>
    /// Ejemplo 11: Obtener decoraciones ordenadas por cantidad
    /// </summary>
    public void ExampleGetMostCommon()
    {
        var decorations = decorationInventory.GetDecorations();
        decorations.Sort((a, b) => b.quantity.CompareTo(a.quantity));
        
        Debug.Log("Decoraciones más comunes:");
        foreach (var decoration in decorations)
        {
            Debug.Log($"{decoration.name}: {decoration.quantity}");
        }
    }
    
    // ===== EJEMPLOS INTERACTIVOS =====
    
    /// <summary>
    /// Ejemplo 12: Teclas de atajo para decoraciones
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            // Colocar decoración con tecla D
            var decoration = decorationInventory.GetDecorations()[0];
            if (decoration != null)
            {
                decorationPlacer.StartPlacing(decoration);
            }
        }
    }
    
    // ===== CLASES AUXILIARES =====
    
    [System.Serializable]
    public class DecorationList
    {
        public System.Collections.Generic.List<DecorationData> decorations;
    }
}

// ===== EJEMPLO DE INTEGRACIÓN CON BOTONES UI =====
/*
 * En tu Canvas, crea un Button y asigna este script OnClick:
 * 
 * Button → OnClick → +
 * Objeto: [DecorationExamples]
 * Función: ExampleAddDecoration(...)
 * 
 * O simplemente llama desde otro script:
 * 
 * DecorationExamples examples = GetComponent<DecorationExamples>();
 * examples.ExampleAddDecoration(myPrefab, myIcon, "Mi Decoración");
 */

// ===== EJEMPLO CON CORRUTINA PARA API =====
/*
 * public IEnumerator FetchDecorationsFromServer(string url)
 * {
 *     using (UnityWebRequest request = UnityWebRequest.Get(url))
 *     {
 *         yield return request.SendWebRequest();
 *         
 *         if (request.result == UnityWebRequest.Result.Success)
 *         {
 *             // Parsear JSON y agregar al inventario
 *             string json = request.downloadHandler.text;
 *             // var data = JsonUtility.FromJson<DecorationList>(json);
 *             // foreach (var decoration in data.decorations)
 *             //     decorationInventory.AddDecoration(decoration);
 *         }
 *     }
 * }
 */
