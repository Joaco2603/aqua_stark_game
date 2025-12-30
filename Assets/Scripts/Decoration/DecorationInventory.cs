using System.Collections.Generic;
using UnityEngine;

public class DecorationInventory : MonoBehaviour
{
    [SerializeField] private List<DecorationData> decorations = new List<DecorationData>();
    
    public List<DecorationData> GetDecorations() => new List<DecorationData>(decorations);
    
    public DecorationData GetDecorationById(int id)
    {
        return decorations.Find(d => d.id == id);
    }
    
    public void AddDecoration(DecorationData decoration)
    {
        var existing = GetDecorationById(decoration.id);
        if (existing != null)
        {
            existing.quantity += decoration.quantity;
        }
        else
        {
            decorations.Add(decoration);
        }
    }
    
    public void RemoveDecoration(int id)
    {
        var decoration = GetDecorationById(id);
        if (decoration != null)
        {
            decoration.quantity--;
            if (decoration.quantity <= 0)
            {
                decorations.Remove(decoration);
            }
        }
    }
    
    public bool HasDecoration(int id)
    {
        return GetDecorationById(id) != null;
    }
}
