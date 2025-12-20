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
    [SerializeField] private float wormLifetime = 5f;
    [SerializeField] private Vector3 fishTankCenter = new Vector3(-2f, 9f, -22.94f);
    [SerializeField] private float zOffset = 0f;

    private GameObject currentWormCursor;
    private bool feedingMode;
    private GameObject spawnedWorm;


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

        // Reemplazar Input.GetMouseButtonDown(0)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceWorm();
        }

        // Reemplazar Input.GetKeyDown(KeyCode.Escape)
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
        
        // Deshabilitar colisiones del cursor si tiene collider
        Collider[] colliders = currentWormCursor.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;
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
        spawnedWorm = Instantiate(wormPrefab, currentWormCursor.transform.position, Quaternion.identity);

        // Habilitar colisiones
        Collider[] colliders = spawnedWorm.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = true;

        // Agregar collider si no tiene ninguno
        if (colliders.Length == 0)
        {
            SphereCollider sc = spawnedWorm.AddComponent<SphereCollider>();
            sc.radius = 0.1f;
        }

        // Agregar rigidbody si no tiene (para que caiga al agua)
        if (spawnedWorm.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = spawnedWorm.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.linearDamping = 0.5f;
        }

        // Destruir el cursor
        Destroy(currentWormCursor);
        currentWormCursor = null;

        // Decirle al pez que vaya a por la comida
        fish.StartFeeding(spawnedWorm.transform);

        // Destruir la lombriz después de un tiempo
        Destroy(spawnedWorm, wormLifetime);

        // Salir del modo alimentación
        EndFeedingMode();
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
