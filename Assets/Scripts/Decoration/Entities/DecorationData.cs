using UnityEngine;


namespace Decoration
{
[System.Serializable]
public class DecorationData
{
    public int id;
    public string name;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    public int quantity;
    
    public DecorationData(int id, string name, string description, Sprite icon, GameObject prefab, int quantity = 1)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.icon = icon;
        this.prefab = prefab;
        this.quantity = quantity;
    }
}
}