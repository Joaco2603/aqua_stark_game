using System.Collections.Generic;
using UnityEngine;

namespace Managers.Entities
{
    [System.Serializable]
    public class AquariumEntity : MonoBehaviour
    {
        public int id;
        public GameObject prefab;

    	public AquariumEntity(int id, GameObject prefab = null)
    	{
    		this.id = id;
            this.prefab = prefab;
    	}
    }
}