using System.Collections.Generic;
using UnityEngine;

namespace Fish.Entities
{
    [System.Serializable]
    public class FishEntity : MonoBehaviour
    {
    	public int id;
    	public string fishName;
    	public int experience;
    	public float hunger;
        public GameObject prefab;

    	public FishEntity(int id, string fishName, int experience, float hunger = 0f, GameObject prefab = null)
    	{
    		this.id = id;
    		this.fishName = fishName;
    		this.experience = experience;
    		this.hunger = hunger;
            this.prefab = prefab;
    	}
    }
}