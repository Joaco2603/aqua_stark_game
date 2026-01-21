using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Decoration;

/// <summary>
/// Lead manager who coordinates the entire decoration system
/// </summary>
public class DecorationManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private DecorationUI decorationUI;
    [SerializeField] private Button deleteButtonPrefab; // Prefab del botón de eliminar
    [SerializeField] private Canvas uiCanvas; // Canvas donde se instanciará el botón

    [Header("Scripts Reference")]
    [SerializeField] private CameraOrbit cameraOrbitScript;
    [SerializeField] private Placement placementScript;

    [Header("Map Settings")]
    [SerializeField] private List<Vector2> mapPositions = new List<Vector2>()
    {
        new Vector2(-200, 90),
        new Vector2(0, 90),
        new Vector2(200, 90),
        new Vector2(-200, -70),
        new Vector2(0, -70),
        new Vector2(200, -70)
    };

    [SerializeField] private bool createMapItemsHidden = true;

    [SerializeField] private GameObject backgroundButtonPrefab;

    // Estado del botón de eliminar
    private Button instantiatedDeleteButton;
    private DecorationController selectedDecoration;
    private Camera mainCamera;

    public static DecorationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (decorationUI != null)
        {
            decorationUI.ValidateFields( decorationUI.GetDecorationDataList() );
        }
        
        // Buscar canvas si no está asignado
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
        }
        
        mainCamera = Camera.main;
        
        // Instanciar el botón de eliminar si existe el prefab
        if (deleteButtonPrefab != null && uiCanvas != null)
        {
            instantiatedDeleteButton = Instantiate(deleteButtonPrefab, uiCanvas.transform);
            instantiatedDeleteButton.gameObject.SetActive(false);
            instantiatedDeleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }
        
        PlaceDecorationsOnMap();
    }

    private void Update()
    {
        // Actualizar posición del botón si hay una decoración seleccionada
        if (selectedDecoration != null && instantiatedDeleteButton != null && instantiatedDeleteButton.gameObject.activeInHierarchy)
        {
            UpdateDeleteButtonPosition();
        }
    }

    
    public void PlaceDecorationsOnMap()
    {
        if (decorationUI == null)
        {
            Debug.LogError("DecorationManager: DecorationUI is not assigned.");
            return;
        }

        for (int i = 0; i < mapPositions.Count; i++)
        {
            // Use a sprite from DecorationUI
            Sprite icon = decorationUI.GetIconAtIndex(i);

                // Crear el item sin ocultarlo aún
                GameObject item = decorationUI.CreateMapItem(mapPositions[i], icon, false, backgroundButtonPrefab);
                
                if (item != null)
                {
                    Button btn = item.GetComponent<Button>();
                    if (btn != null)
                    {
                        GameObject prefab = decorationUI.GetPrefabForIcon(icon);
                        if (prefab != null && cameraOrbitScript != null)
                        {
                            btn.onClick.AddListener(() => {
                                decorationUI.SetVisible(false);
                                placementScript.StartPlacing(prefab, cameraOrbitScript.Target, () => {
                                    decorationUI.SetVisible(true);
                                    // Cuando se coloca la decoración, configurar el seguimiento
                                    SetupPlacedDecoration();
                                });
                            });
                        }
                    }
                    
                    // Ocultar después de asignar el listener
                    if (createMapItemsHidden)
                    {
                        item.SetActive(false);
                    }
                }
        }
    }

    /// <summary>
    /// Configura la decoración recién colocada para el seguimiento
    /// </summary>
    private void SetupPlacedDecoration()
    {
        // Buscar la última decoración colocada
        DecorationController[] decorations = FindObjectsOfType<DecorationController>();
        if (decorations.Length > 0)
        {
            // Tomar la última (la recién creada)
            DecorationController newDecoration = decorations[decorations.Length - 1];
            SelectDecoration(newDecoration);
        }
    }

    /// <summary>
    /// Selecciona una decoración y muestra el botón de eliminar
    /// </summary>
    public void SelectDecoration(DecorationController decoration)
    {
        selectedDecoration = decoration;
        
        if (instantiatedDeleteButton != null)
        {
            instantiatedDeleteButton.gameObject.SetActive(true);
            UpdateDeleteButtonPosition();
        }
    }

    /// <summary>
    /// Deselecciona la decoración actual
    /// </summary>
    public void DeselectDecoration()
    {
        selectedDecoration = null;
        
        if (instantiatedDeleteButton != null)
        {
            instantiatedDeleteButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Actualiza la posición del botón de eliminar para seguir al objeto 3D
    /// </summary>
    private void UpdateDeleteButtonPosition()
    {
        if (selectedDecoration == null || instantiatedDeleteButton == null || mainCamera == null || uiCanvas == null)
            return;

        // Convertir posición 3D del objeto a posición en pantalla
        Vector3 worldPos = selectedDecoration.transform.position;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // Verificar que el objeto está frente a la cámara
        if (screenPos.z < 0)
        {
            instantiatedDeleteButton.gameObject.SetActive(false);
            return;
        }

        // Convertir posición de pantalla a posición en el Canvas
        RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiCanvas.worldCamera,
            out Vector2 localPos
        );

        // Añadir offset hacia arriba para que aparezca sobre el objeto
        localPos.y += 50f;

        // Asignar la posición al botón
        RectTransform buttonRect = instantiatedDeleteButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.anchoredPosition = localPos;
        }
    }

    /// <summary>
    /// Maneja el clic en el botón de eliminar
    /// </summary>
    private void OnDeleteButtonClicked()
    {
        if (selectedDecoration != null)
        {
            Destroy(selectedDecoration.gameObject);
            DeselectDecoration();
        }
    }
}
