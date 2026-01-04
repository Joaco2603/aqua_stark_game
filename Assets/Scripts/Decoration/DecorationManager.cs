using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Lead manager who coordinates the entire decoration system
/// </summary>
public class DecorationManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private DecorationUI decorationUI;

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

    // [SerializeField] private List<Sprite> decorationSprites; // Removed to avoid duplication

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
                decorationUI.CreateMapItem(mapPositions[i], icon, createMapItemsHidden);
            }
        }
    }
}
