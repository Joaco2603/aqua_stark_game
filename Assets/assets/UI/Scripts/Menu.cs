using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public void Open()
    {
        gameObject.SetActive(true);
        SetChildrenActive(true);
        SetCanvasesEnabled(true);
        SetGraphicsEnabled(true);
    }

    public void Close()
    {
        // Disable canvases/graphics first to ensure UI stops rendering, then deactivate children and parent
        SetCanvasesEnabled(false);
        SetGraphicsEnabled(false);
        SetChildrenActive(false);
        gameObject.SetActive(false);
    }

    void SetChildrenActive(bool active)
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(active);
    }

    void SetCanvasesEnabled(bool enabled)
    {
        var canvases = GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases)
            c.enabled = enabled;
    }

    void SetGraphicsEnabled(bool enabled)
    {
        var graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
            g.enabled = enabled;
    }
}
