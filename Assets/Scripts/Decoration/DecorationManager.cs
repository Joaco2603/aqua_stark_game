using UnityEngine;

/// <summary>
/// Manager principal que coordina todo el sistema de decoraciones
/// </summary>
public class DecorationManager : MonoBehaviour
{
    [SerializeField] private DecorationInventory decorationInventory;
    [SerializeField] private DecorationPlacer decorationPlacer;
    [SerializeField] private DecorationUI decorationUI;
    
    void Awake()
    {
        // Asegurar que los componentes existan
        if (decorationInventory == null)
            decorationInventory = GetComponent<DecorationInventory>();
        
        if (decorationPlacer == null)
            decorationPlacer = GetComponent<DecorationPlacer>();
        
        if (decorationUI == null)
            decorationUI = GetComponent<DecorationUI>();
    }
    
    /// <summary>
    /// Inicializa el inventario con decoraciones de prueba
    /// </summary>
    public void InitializeWithTestData()
    {
        if (decorationInventory == null) return;
        
        // Aquí puedes agregar decoraciones de prueba
        // Esto debería ser reemplazado con datos reales del servidor o archivos de configuración
    }
}
