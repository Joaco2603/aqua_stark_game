using System.Collections.Generic;
using UnityEngine;

namespace Fish.Reproduce.Utils
{
	public class FindAllFishes : MonoBehaviour
	{
		[SerializeField] private GameObject FishContainer;

		// Devuelve todos los GameObjects que están dentro de FishContainer (recursivo)
		public GameObject[] FindAll()
		{
			if (FishContainer == null) return new GameObject[0];

			var list = new List<GameObject>();
			foreach (Transform child in FishContainer.transform)
			{
				AddRecursive(child, list);
			}

			return list.ToArray();
		}

		private void AddRecursive(Transform node, List<GameObject> outList)
		{
			outList.Add(node.gameObject);
			foreach (Transform child in node)
			{
				AddRecursive(child, outList);
			}
		}

		// Método accesible desde el menú contextual del componente en el Inspector
		[ContextMenu("Find All Fishes")]
		private void FindAndLog()
		{
			var all = FindAll();
			Debug.Log($"Found {all.Length} objects under {(FishContainer != null ? FishContainer.name : "null")}");
		}
	}
}

