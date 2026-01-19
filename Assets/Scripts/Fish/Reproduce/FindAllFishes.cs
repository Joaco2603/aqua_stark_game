using System.Collections.Generic;
using UnityEngine;
using Fish.Entities;

namespace Fish.Reproduce
{
	public class FindAllFishes : MonoBehaviour
	{
		[SerializeField] private GameObject FishContainer;

		// Última lista encontrada; accesible desde otros scripts
		public GameObject[] LastFoundFishes { get; private set; } = new GameObject[0];

		private void Awake()
		{
			if (FishContainer == null)
			{
				FishContainer = gameObject;
			}
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
        // Iniciamos la búsqueda desde los hijos directos del contenedor
        AddRecursiveStopAtMatch(child, list);
    }

    LastFoundFishes = list.ToArray();
    return LastFoundFishes;
}

private void AddRecursiveStopAtMatch(Transform current, List<GameObject> list)
{
    // Intentamos obtener el componente en el objeto actual
    FishEntity fishEntity = current.GetComponent<FishEntity>();

    if (fishEntity != null)
    {
        // ¡Lo encontramos! Lo añadimos a la lista
        list.Add(current.gameObject);
        
        // IMPORTANTE: Al no llamar a AddRecursiveStopAtMatch aquí, 
        // ignoramos por completo a sus hijos y evitamos duplicados.
    }
    else
    {
        // Si este objeto NO es un pez, revisamos a sus hijos
        // para ver si alguno de ellos lo es.
        foreach (Transform child in current)
        {
            AddRecursiveStopAtMatch(child, list);
        }
    }
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

