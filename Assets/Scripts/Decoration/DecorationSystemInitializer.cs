using UnityEngine;

/// <summary>
/// Script auxiliar para inicializar decoraciones de prueba
/// Reemplaza esto con datos reales de tu servidor/configuración
/// </summary>
public class DecorationSystemInitializer : MonoBehaviour
{
    [SerializeField] private DecorationInventory decorationInventory;
    [SerializeField] private Sprite[] decorationIcons;
    [SerializeField] private GameObject[] decorationPrefabs;
    
    void Start()
    {
        if (decorationInventory == null)
            decorationInventory = FindObjectOfType<DecorationInventory>();
        
        InitializeTestDecorations();
    }
    
    private void InitializeTestDecorations()
    {
        // Ejemplo: Agregar decoraciones de prueba
        // Reemplaza esto con datos reales
        
        if (decorationPrefabs.Length > 0 && decorationIcons.Length > 0)
        {
            for (int i = 0; i < decorationPrefabs.Length; i++)
            {
                if (i < decorationIcons.Length && decorationPrefabs[i] != null)
                {
                    decorationInventory.AddDecoration(new DecorationData(
                        id: i,
                        name: $"Decoración {i}",
                        description: $"Una hermosa decoración número {i}",
                        icon: decorationIcons[i],
                        prefab: decorationPrefabs[i],
                        quantity: 3
                    ));
                }
            }
        }
    }
    
    /// <summary>
    /// Método público para agregar decoraciones dinámicamente
    /// </summary>
    public void AddDecoration(int id, string name, Sprite icon, GameObject prefab, int quantity = 1)
    {
        if (decorationInventory != null)
        {
            decorationInventory.AddDecoration(new DecorationData(id, name, "", icon, prefab, quantity));
        }
    }
}
