using UnityEngine;

/// <summary>
/// Componente que permite interactuar con decoraciones colocadas (mover, rotar, eliminar)
/// </summary>
public class DecorationObject : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private Plane dragPlane;
    private Camera mainCamera;
    private float dragHeight;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void OnMouseDown()
    {
        isDragging = true;
        dragHeight = transform.position.y;
        dragPlane = new Plane(Vector3.up, dragHeight);
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.origin + ray.direction * distance;
            offset = transform.position - hitPoint;
        }
    }
    
    void OnMouseDrag()
    {
        if (!isDragging) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.origin + ray.direction * distance;
            transform.position = hitPoint + offset;
        }
    }
    
    void OnMouseUp()
    {
        isDragging = false;
    }
    
    /// <summary>
    /// Permite rotar la decoración con las teclas Q y E
    /// </summary>
    void Update()
    {
        if (!isDragging) return;
        
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -5f, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, 5f, 0);
        }
    }
    
    public void Delete()
    {
        Destroy(gameObject);
    }
}
