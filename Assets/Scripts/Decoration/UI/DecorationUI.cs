using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DecorationUI : MonoBehaviour
{
	[SerializeField] private Canvas targetCanvas;
	[SerializeField] private Transform mapContainer;
	// [SerializeField] private List<Button> buttonPrefabs = new List<Button>();
	private Transform defaultParent;

	[System.Serializable]
	public struct IconPrefabPair
	{
		public Sprite icon;
		public GameObject prefab;
	}

	[SerializeField] private List<IconPrefabPair> iconPrefabPairs = new List<IconPrefabPair>();

	// Runtime lookup (not serialized) construido desde la lista anterior
	private Dictionary<Sprite, GameObject> iconToPrefabLookup;

	private void EnsureIconLookup()
	{
		if (iconToPrefabLookup != null) return;
		iconToPrefabLookup = new Dictionary<Sprite, GameObject>();
		foreach (var p in iconPrefabPairs)
		{
			if (p.icon == null) continue;
			if (!iconToPrefabLookup.ContainsKey(p.icon))
				iconToPrefabLookup.Add(p.icon, p.prefab);
			else
				iconToPrefabLookup[p.icon] = p.prefab;
		}
	}

	public GameObject GetPrefabForIcon(Sprite icon)
	{
		EnsureIconLookup();
		if (icon == null) return null;
		iconToPrefabLookup.TryGetValue(icon, out var prefab);
		return prefab;
	}

    public int GetDecorationCount()
    {
        return iconPrefabPairs.Count;
    }

    public Sprite GetIconAtIndex(int index)
    {
        if (index >= 0 && index < iconPrefabPairs.Count)
        {
            return iconPrefabPairs[index].icon;
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
