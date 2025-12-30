using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DecorationUI : MonoBehaviour
{
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private DecorationInventory decorationInventory;
    [SerializeField] private DecorationPlacer decorationPlacer;
    
    private Dictionary<int, GameObject> uiItems = new Dictionary<int, GameObject>();
    
    void Start()
    {
        if (decorationInventory == null)
            decorationInventory = FindObjectOfType<DecorationInventory>();
        
        if (decorationPlacer == null)
            decorationPlacer = FindObjectOfType<DecorationPlacer>();
        
        RefreshInventoryUI();
    }
    
    public void RefreshInventoryUI()
    {
        // Limpiar UI anterior
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }
        uiItems.Clear();
        
        // Crear items del inventario
        var decorations = decorationInventory.GetDecorations();
        foreach (var decoration in decorations)
        {
            CreateInventoryItem(decoration);
        }
    }
    
    private void CreateInventoryItem(DecorationData decoration)
    {
        var itemUI = Instantiate(inventoryItemPrefab, inventoryGrid);
        itemUI.name = $"Item_{decoration.name}";
        
        // Asignar icono
        var image = itemUI.GetComponent<Image>();
        if (image != null && decoration.icon != null)
        {
            image.sprite = decoration.icon;
        }
        
        // Asignar cantidad
        var quantityText = itemUI.GetComponentInChildren<Text>();
        if (quantityText != null)
        {
            quantityText.text = decoration.quantity > 1 ? decoration.quantity.ToString() : "";
        }
        
        // Asignar botón
        var button = itemUI.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnDecorationSelected(decoration));
        }
        
        uiItems[decoration.id] = itemUI;
    }
    
    private void OnDecorationSelected(DecorationData decoration)
    {
        if (decoration.quantity > 0)
        {
            decorationPlacer.StartPlacing(decoration);
        }
        else
        {
            Debug.LogWarning($"No hay más {decoration.name} en el inventario");
        }
    }
}
