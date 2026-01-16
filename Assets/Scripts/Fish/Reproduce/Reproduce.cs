using UnityEngine;
using Fish.Entities;

public class Reproduce : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    FishEntity CreateFish(int id, string name, int experience, float hunger = 0f, GameObject prefab = null)
    {
        return new FishEntity(id, name, experience, hunger, prefab);
    }
}
