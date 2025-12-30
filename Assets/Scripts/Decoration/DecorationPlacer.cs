using UnityEngine;

public class DecorationPlacer : MonoBehaviour
{
    [SerializeField] private Transform aquariumParent; // El transform donde se colocan las decoraciones
    [SerializeField] private float placementHeight = 0f;
    [SerializeField] private bool useGridSnapping = true;
    [SerializeField] private float gridSize = 0.5f;
    
    private GameObject currentPreview;
    private bool isPlacing = false;
    private int selectedDecorationId = -1;
    private DecorationData selectedDecoration;
    private Camera mainCamera;
    private Plane groundPlane;
    
    void Start()
    {
        mainCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, placementHeight);
    }
    
    void Update()
    {
        if (!isPlacing) return;
        
        UpdatePreviewPosition();
        
        if (Input.GetMouseButtonDown(0))
        {
            PlaceDecoration();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }
    
    /// <summary>
    /// Inicia el proceso de colocación de una decoración
    /// </summary>
    public void StartPlacing(DecorationData decoration)
    {
        if (decoration.prefab == null)
        {
            Debug.LogError($"La decoración {decoration.name} no tiene prefab asignado");
            return;
        }
        
        selectedDecoration = decoration;
        selectedDecorationId = decoration.id;
        isPlacing = true;
        
        // Crear preview
        currentPreview = Instantiate(decoration.prefab);
        SetPreviewMaterial(currentPreview);
        
        // Hacer el preview semi-transparente
        SetObjectTransparency(currentPreview, 0.5f);
    }
    
    private void UpdatePreviewPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.origin + ray.direction * distance;
            
            if (useGridSnapping)
            {
                hitPoint = SnapToGrid(hitPoint);
            }
            
            if (currentPreview != null)
            {
                currentPreview.transform.position = hitPoint;
            }
        }
    }
    
    private Vector3 SnapToGrid(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            position.y,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }
    
    private void PlaceDecoration()
    {
        if (currentPreview == null) return;
        
        // Hacer la decoración opaca
        SetObjectTransparency(currentPreview, 1f);
        
        // Cambiar material del objeto final
        ResetObjectMaterials(currentPreview);
        
        // Asignar al padre (aquarium)
        if (aquariumParent != null)
        {
            currentPreview.transform.SetParent(aquariumParent);
        }
        
        // Agregar componente de interacción si es necesario
        AddDecorationInteraction(currentPreview);
        
        // Notificar que se colocó
        OnDecorationPlaced();
        
        currentPreview = null;
    }
    
    private void CancelPlacement()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
        isPlacing = false;
        selectedDecorationId = -1;
        selectedDecoration = null;
    }
    
    private void SetPreviewMaterial(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            var materials = new Material[renderer.materials.Length];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                materials[i] = new Material(renderer.materials[i])
                {
                    renderQueue = 3000
                };
            }
            renderer.materials = materials;
        }
    }
    
    private void ResetObjectMaterials(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            // Restaurar materiales originales o aplicar normales
            if (selectedDecoration != null && selectedDecoration.prefab != null)
            {
                var prefabRenderer = selectedDecoration.prefab.GetComponent<Renderer>();
                if (prefabRenderer != null)
                {
                    renderer.materials = prefabRenderer.sharedMaterials;
                }
            }
        }
    }
    
    private void SetObjectTransparency(GameObject obj, float alpha)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.SetFloat("_Alpha", alpha);
                
                // Si el material soporta transparencia
                if (alpha < 1f)
                {
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.renderQueue = 3000;
                }
            }
        }
    }
    
    private void AddDecorationInteraction(GameObject decoration)
    {
        // Agregar componente para poder mover la decoración después
        if (decoration.GetComponent<DecorationObject>() == null)
        {
            decoration.AddComponent<DecorationObject>();
        }
    }
    
    private void OnDecorationPlaced()
    {
        // Consumir del inventario
        var inventoryManager = FindObjectOfType<DecorationInventory>();
        if (inventoryManager != null)
        {
            inventoryManager.RemoveDecoration(selectedDecorationId);
        }
        
        // Notificar a la UI
        var decorationUI = FindObjectOfType<DecorationUI>();
        if (decorationUI != null)
        {
            decorationUI.RefreshInventoryUI();
        }
        
        isPlacing = false;
        selectedDecorationId = -1;
        selectedDecoration = null;
    }
    
    public bool IsPlacing => isPlacing;
    public int GetSelectedDecorationId => selectedDecorationId;
}
