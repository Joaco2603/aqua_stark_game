using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DecorationUI : MonoBehaviour
{
	[SerializeField] private Canvas targetCanvas;
	[SerializeField] private Transform mapContainer;
	[SerializeField] private List<Button> buttonPrefabs = new List<Button>();
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
		foreach (var prefab in buttonPrefabs)
		{
			if (prefab == null) continue;
			Button inst = Instantiate(prefab, parentTransform);
			created.Add(inst);
		}

		return created;
	}

	// Now returns the created GameObject and optionally creates it hidden (inactive).
	public GameObject CreateMapItem(Vector2 position, Sprite icon, bool createHidden = false)
	{
		if (mapContainer == null)
		{
			Debug.LogWarning("DecorationUI: Map Container is not assigned.");
			return null;
		}

		GameObject item = new GameObject("MapDecoration_" + (icon != null ? icon.name : "null"));
		item.transform.SetParent(mapContainer, false);

		Image img = item.AddComponent<Image>();
		img.sprite = icon;
		img.preserveAspect = true;

		RectTransform rt = item.GetComponent<RectTransform>();
		rt.anchoredPosition = position;
		rt.sizeDelta = new Vector2(100, 100); // Default size

		if (createHidden)
		{
			item.SetActive(false);
			mapContainer.gameObject.SetActive(false);
		}

		return item;
	}
}
