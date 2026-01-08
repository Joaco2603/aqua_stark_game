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
        PlaceDecorationsOnMap();
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
}
