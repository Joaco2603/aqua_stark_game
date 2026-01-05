using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Decoration;

/// <summary>
/// Lead manager who coordinates the entire decoration system
/// </summary>
public class DecorationManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private DecorationUI decorationUI;
    [SerializeField] private CameraOrbit cameraOrbit;

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

    private void Start()
    {
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
            
            if (icon == null)
            {
                // Only warn if we expect an icon for this position (i.e. if we have fewer icons than positions)
                if (i < decorationUI.GetDecorationCount())
                {
                     Debug.LogWarning($"Posición {i} ({mapPositions[i]}): El icono es null en 'Icon Prefab Pairs' de DecorationUI.");
                }
                // If we just ran out of icons, maybe that's intended, or we can warn:
                // Debug.LogWarning($"Posición {i}: No hay suficientes elementos en 'Icon Prefab Pairs' para cubrir esta posición.");
            }
            else
            {
                // Crear el item sin ocultarlo aún
                GameObject item = decorationUI.CreateMapItem(mapPositions[i], icon, false, backgroundButtonPrefab);
                
                if (item != null)
                {
                    Button btn = item.GetComponent<Button>();
                    if (btn != null)
                    {
                        GameObject prefab = decorationUI.GetPrefabForIcon(icon);
                        if (prefab != null && cameraOrbit != null)
                        {
                            btn.onClick.AddListener(() => {
                                decorationUI.SetVisible(false);
                                cameraOrbit.StartPlacing(prefab, () => {
                                    decorationUI.SetVisible(true);
                                });
                            });
                        }
                        else
                        {
                            Debug.LogWarning($"Prefab o CameraOrbit es null para el icono: {icon.name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"El item no tiene componente Button: {item.name}");
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
}
