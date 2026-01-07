using UnityEngine;
using UnityEngine.UI;
using Decoration;
using System.Collections.Generic;

public class DecorationUI : MonoBehaviour
{
	[SerializeField] private Canvas targetCanvas;
	[SerializeField] private Transform mapContainer;
    [SerializeField] private Transform defaultParent;

	[SerializeField] private List<DecorationData> decorationData = new List<DecorationData>();

	void Start()
	{
		ValidateFields(decorationData);
	}

	public void ValidateFields( List<DecorationData> data )
    {
		foreach(var item in data)
		{
            if (item.prefab == null)
            {
                Debug.LogWarning($"DecorationData ID {item.id} ('{item.name}') tiene un prefab null.");
            }
            if (item.icon == null)
            {
                Debug.LogWarning($"DecorationData ID {item.id} ('{item.name}') tiene un icono null.");
            }
            if (string.IsNullOrEmpty(item.name))
            {
                Debug.LogWarning($"DecorationData ID {item.id} tiene un nombre vacío.");
            }
            if(item.quantity < 0)
            {
                Debug.LogWarning($"DecorationData ID {item.id} ('{item.name}') tiene una cantidad negativa: {item.quantity}.");
            }
            if(item.description == null)
            {
                Debug.LogWarning($"DecorationData ID {item.id} ('{item.name}') tiene una descripción null.");
            }
		}
    }

    public List<DecorationData> GetDecorationDataList()
    {
        return decorationData;
    }

	public GameObject GetPrefabForIcon(Sprite icon)
	{
        foreach (var data in decorationData)
        {
            if (data.icon == icon)
            {
                return data.prefab;
            }
        }
        return null;
	}

    public int GetDecorationCount()
    {
        return decorationData.Count;
    }

    public Sprite GetIconAtIndex(int index)
    {
        if (index >= 0 && index < decorationData.Count)
        {
            return decorationData[index].icon;
        }
        return null;
    }

	public List<Button> CreateButtons(Transform parent = null)
	{
		Transform parentTransform = parent != null ? parent : (defaultParent != null ? defaultParent : (targetCanvas != null ? targetCanvas.transform : null));
		if (parentTransform == null)
		{
			Debug.LogWarning("DecorationUI: No parent provided and no default parent/canvas available.");
			return new List<Button>();
		}

		List<Button> created = new List<Button>();
		// foreach (var prefab in buttonPrefabs)
		// {
		// 	if (prefab == null) continue;
		// 	Button inst = Instantiate(prefab, parentTransform);
		// 	created.Add(inst);
		// }

		return created;
	}

	// Now returns the created GameObject and optionally creates it hidden (inactive).
	public GameObject CreateMapItem(Vector2 position, Sprite icon, bool createHidden = false, GameObject buttonPrefab = null)
	{
		if (mapContainer == null)
		{
			Debug.LogWarning("DecorationUI: Map Container is not assigned.");
			return null;
		}

		GameObject item;
		if (buttonPrefab != null)
		{
			item = Instantiate(buttonPrefab, mapContainer);
			item.name = "MapDecoration_" + (icon != null ? icon.name : "null");
		}
		else
		{
			item = new GameObject("MapDecoration_" + (icon != null ? icon.name : "null"));
			item.transform.SetParent(mapContainer, false);
		}

		RectTransform rt = item.GetComponent<RectTransform>();
		if (rt != null)
		{
			rt.anchoredPosition = position;
			if (buttonPrefab == null)
			{
				rt.sizeDelta = new Vector2(100, 100); // Default size
			}
		}

		if (buttonPrefab != null)
		{
			// Create icon as child so it appears on top of the button
			GameObject iconObj = new GameObject("Icon");
			iconObj.transform.SetParent(item.transform, false);

			Image img = iconObj.AddComponent<Image>();
			img.sprite = icon;
			img.preserveAspect = true;

			RectTransform iconRt = iconObj.GetComponent<RectTransform>();
			iconRt.anchoredPosition = Vector2.zero;
			iconRt.sizeDelta = new Vector2(100, 100);
		}
		else
		{
			Image img = item.AddComponent<Image>();
			img.sprite = icon;
			img.preserveAspect = true;
		}

		// Asegurar que tenga un componente Button
		if (item.GetComponent<Button>() == null)
		{
			item.AddComponent<Button>();
		}

		return item;
	}

    public void SetVisible(bool visible)
    {
        if (mapContainer != null)
        {
            mapContainer.gameObject.SetActive(visible);
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }
}
