using System.Collections.Generic;
using UnityEngine;

namespace Fish.Reproduce
{
	public class FindAllFishes : MonoBehaviour
	{
		[SerializeField] private GameObject FishContainer;

		// Última lista encontrada; accesible desde otros scripts
		public GameObject[] LastFoundFishes { get; private set; } = new GameObject[0];

		private void Awake()
		{
			FishContainer = gameObject;
		}

		public bool validateCountFishes(int minCount)
		{
			var fishes = FindAll();
			return fishes.Length >= minCount;
		}

		// Devuelve todos los GameObjects que están dentro de FishContainer (recursivo)
		public GameObject[] FindAll()
		{
			if (FishContainer == null) return new GameObject[0];

			var list = new List<GameObject>();
			foreach (Transform child in FishContainer.transform)
			{
				AddRecursive(child, list);
			}

			LastFoundFishes = list.ToArray();
			return LastFoundFishes;
		}

		// Actualiza la lista y la devuelve (útil para otros scripts)
		public GameObject[] RefreshFoundFishes()
		{
			return FindAll();
		}

		private void AddRecursive(Transform node, List<GameObject> outList)
		{
			outList.Add(node.gameObject);
			foreach (Transform child in node)
			{
				AddRecursive(child, outList);
			}
		}
	}
}

