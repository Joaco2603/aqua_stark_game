using UnityEngine;
using UnityEngine.InputSystem;


public class FeedFish : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private FishMove fish;
    [SerializeField] private GameObject wormPrefab;
    [SerializeField] private GameObject uiToHide;
    [SerializeField] private Camera mainCamera;
    
    [Header("Configuración")]
    [SerializeField] private float wormFollowDistance = 10f;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private Vector3 fishTankCenter = new Vector3(-2f, 9f, -22.94f);
    [SerializeField] private float zOffset = 0f;
    
    [Header("Cursor Personalizado")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 cursorHotspot = new Vector2(0, 0);

    private GameObject currentWormCursor;
    private bool feedingMode;
    private Texture2D originalCursor;


    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!feedingMode)
            return;

        UpdateWormCursor();

        // Click izquierdo para colocar alimento
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceWorm();
        }

        // Solo Escape para salir del modo alimentación
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelFeeding();
        }
    }

    public void Feed()
    {
        if (fish == null && !TryGetComponent(out fish))
        {
            Debug.LogWarning("FeedFish no encontró un FishMove en el mismo objeto", this);
            return;
        }

        if (wormPrefab == null)
        {
            Debug.LogWarning("No hay prefab de lombriz asignado", this);
            return;
        }

        StartFeedingMode();
    }

    private void StartFeedingMode()
    {
        feedingMode = true;

        // Ocultar UI
        if (uiToHide != null)
            uiToHide.SetActive(false);

        // Crear cursor de lombriz
        currentWormCursor = Instantiate(wormPrefab);

        Rigidbody rb = currentWormCursor.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // El cursor debe seguir al mouse, no a la física
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.Sleep();
        }
        
        // Deshabilitar colisiones del cursor si tiene collider
        Collider[] colliders = currentWormCursor.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;
        
        // Cambiar cursor del mouse si hay textura personalizada
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }
    }

    private void UpdateWormCursor()
    {
        if (currentWormCursor == null || Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Vector3 targetPosition;
        if (waterLayer != 0 && Physics.Raycast(ray, out RaycastHit hit, 100f, waterLayer))
        {
            targetPosition = hit.point;
        }
        else
        {
            targetPosition = ray.GetPoint(wormFollowDistance);
        }

        // Aplicar sesgo en Z
        targetPosition.z += zOffset;

        currentWormCursor.transform.position = targetPosition;
    }


    private void PlaceWorm()
    {
        if (currentWormCursor == null)
            return;

        // Spawnear lombriz real en la posición del cursor
        GameObject spawnedWorm = Instantiate(wormPrefab, currentWormCursor.transform.position, Quaternion.identity);

        // Habilitar colisiones
        Collider[] colliders = spawnedWorm.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = true;

        // Agregar collider si no tiene ninguno
        if (colliders.Length == 0)
        {
            SphereCollider sc = spawnedWorm.AddComponent<SphereCollider>();
            sc.radius = 0.5f; // Radio más grande para mejor detección
            sc.isTrigger = false; // NO trigger, collider normal
        }
        else
        {
            // Asegurar que los colliders NO sean triggers para colisiones reales
            foreach (var col in colliders)
                col.isTrigger = false;
        }

        // Agregar rigidbody para que caiga al agua y sea detectable por la física
        if (spawnedWorm.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = spawnedWorm.AddComponent<Rigidbody>();
            rb.useGravity = true; // Activar gravedad para que caiga
            rb.isKinematic = false; // Permitimos respuesta física
            rb.linearDamping = 0.5f; // Resistencia moderada
            rb.angularDamping = 0.5f;
        }
        else
        {
            // Si ya tiene rigidbody, configurarlo
            Rigidbody rb = spawnedWorm.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.WakeUp();
        }
        
        // Agregar tag de Food si no lo tiene
        if (string.IsNullOrEmpty(spawnedWorm.tag) || spawnedWorm.tag == "Untagged")
        {
            spawnedWorm.tag = "Food";
        }

        // Salir automáticamente del modo alimentación después de colocar un alimento
        CancelFeeding();
    }

    private void CancelFeeding()
    {
        if (currentWormCursor != null)
            Destroy(currentWormCursor);

        EndFeedingMode();
    }

    private void EndFeedingMode()
    {
        feedingMode = false;

        // Restaurar cursor normal
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // Mostrar UI de nuevo
        if (uiToHide != null)
            uiToHide.SetActive(true);
    }

    private void OnDisable()
    {
        if (currentWormCursor != null)
            Destroy(currentWormCursor);
    }
}
