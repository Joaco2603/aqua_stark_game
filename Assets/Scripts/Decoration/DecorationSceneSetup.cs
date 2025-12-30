using UnityEngine;

/// <summary>
/// Ejemplo de cómo integrar el sistema de decoración con tu escena de acuario
/// 
/// Paso a paso:
/// 1. Copia este script a tu escena Decoration.unity
/// 2. Crea un GameObject vacío llamado "DecorationSystem"
/// 3. Asigna este script como componente
/// 4. En el inspector, asigna los elementos necesarios
/// 5. Juega la escena
/// </summary>
public class DecorationSceneSetup : MonoBehaviour
{
    [Header("Referencias de la escena")]
    [SerializeField] private Transform aquariumTransform;
    [SerializeField] private Canvas uiCanvas;
    
    [Header("Prefabs de decoraciones")]
    [SerializeField] private GameObject[] decorationPrefabs;
    [SerializeField] private Sprite[] decorationIcons;
    [SerializeField] private string[] decorationNames;
    
    [Header("Componentes del sistema")]
    private DecorationManager decorationManager;
    private DecorationInventory decorationInventory;
    private DecorationPlacer decorationPlacer;
    private DecorationUI decorationUI;
    
    void Start()
    {
        SetupDecorationSystem();
        InitializeInventory();
    }
    
    private void SetupDecorationSystem()
    {
        // Obtener o crear componentes
        decorationManager = GetComponent<DecorationManager>();
        if (decorationManager == null)
            decorationManager = gameObject.AddComponent<DecorationManager>();
        
        decorationInventory = GetComponent<DecorationInventory>();
        if (decorationInventory == null)
            decorationInventory = gameObject.AddComponent<DecorationInventory>();
        
        decorationPlacer = GetComponent<DecorationPlacer>();
        if (decorationPlacer == null)
            decorationPlacer = gameObject.AddComponent<DecorationPlacer>();
        
        decorationUI = GetComponentInChildren<DecorationUI>();
        if (decorationUI == null)
        {
            // Crear UI si no existe
            GameObject uiObject = new GameObject("DecorationUIPanel");
            uiObject.transform.SetParent(uiCanvas.transform);
            decorationUI = uiObject.AddComponent<DecorationUI>();
        }
        
        Debug.Log("✅ Sistema de decoración inicializado");
    }
    
    private void InitializeInventory()
    {
        if (decorationPrefabs.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay prefabs asignados. Asigna decorationPrefabs en el inspector");
            return;
        }
        
        // Agregar decoraciones al inventario
        for (int i = 0; i < decorationPrefabs.Length && i < decorationIcons.Length; i++)
        {
            if (decorationPrefabs[i] != null && decorationIcons[i] != null)
            {
                string name = (i < decorationNames.Length && decorationNames[i] != null) 
                    ? decorationNames[i] 
                    : $"Decoración {i}";
                
                decorationInventory.AddDecoration(new DecorationData(
                    id: i,
                    name: name,
                    description: $"Una hermosa {name}",
                    icon: decorationIcons[i],
                    prefab: decorationPrefabs[i],
                    quantity: 5
                ));
            }
        }
        
        Debug.Log($"✅ {decorationPrefabs.Length} decoraciones cargadas al inventario");
    }
    
    /// <summary>
    /// Método público para resetear todas las decoraciones colocadas
    /// </summary>
    public void ResetAllDecorations()
    {
        if (aquariumTransform == null) return;
        
        var decorationObjects = aquariumTransform.GetComponentsInChildren<DecorationObject>();
        foreach (var decObj in decorationObjects)
        {
            Destroy(decObj.gameObject);
        }
        
        // Recargar inventario
        InitializeInventory();
        
        Debug.Log("✅ Decoraciones reseteadas");
    }
}

/*
 * EJEMPLO DE USO EN TU ESCENA Decoration.unity
 * 
 * 1. Crea un GameObject vacío llamado "DecorationSystem"
 * 2. Asigna este script como componente
 * 3. En el inspector, completa:
 *    - aquariumTransform: Asigna tu Transform del acuario
 *    - uiCanvas: Asigna tu Canvas principal
 *    - decorationPrefabs[]: Array con tus prefabs de decoración
 *    - decorationIcons[]: Array con los iconos para la UI
 *    - decorationNames[]: Array con nombres (opcional)
 * 
 * 4. Presiona Play y debería funcionar
 * 
 * Si tienes problemas:
 * - Verifica que los prefabs tengan Renderer
 * - Asegúrate de que el Canvas esté configurado correctamente
 * - Revisa la consola para mensajes de debug
 */
