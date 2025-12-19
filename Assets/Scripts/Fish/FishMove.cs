using UnityEngine; 

[RequireComponent(typeof(Rigidbody))] 
public class FishMove : MonoBehaviour 
{ 
    [Header("Swim Behaviour")] 
    [SerializeField] private float swimSpeed = 2.5f; 
    [SerializeField] private float turnLerp = 3f; 
    [SerializeField] private float directionChangeInterval = 2.5f; 
    [SerializeField] private float wanderRadius = 6f; 
    [SerializeField] private float verticalBias = 0.35f; 
 
    [Header("Water Bounds")] 
    [SerializeField] private float waterLevel = 0f; 
    [SerializeField] private float waterDepth = 5f; // Nueva variable más clara
    [SerializeField] private float surfaceOffset = 0.3f; 
    [SerializeField] private float bottomOffset = 0.5f; 
    [SerializeField] private bool clampToWater = true; 
    [SerializeField] private bool resetOnEnable = true; 
 
    private Rigidbody rb; 
    private Vector3 initialPosition; 
    private Vector3 centerPoint; 
    private Vector3 currentDirection; 
    private float changeTimer; 
 
    private void Awake() 
    { 
        rb = GetComponent<Rigidbody>();
        
        // IMPORTANTE: Desactivar gravedad
        rb.useGravity = false;
        rb.linearDamping = 1f; // Añade un poco de resistencia
        
        initialPosition = transform.position; 
        centerPoint = transform.position; 
        PickNewDirection(true); 
    } 
 
    private void OnEnable() 
    { 
        if (resetOnEnable) 
        { 
            transform.position = initialPosition; 
            if (rb != null) 
            { 
                rb.linearVelocity = Vector3.zero; 
                rb.angularVelocity = Vector3.zero; 
            } 
            centerPoint = initialPosition; 
            PickNewDirection(true); 
        } 
    } 
 
    private void FixedUpdate() 
    { 
        changeTimer -= Time.fixedDeltaTime; 
 
        if (changeTimer <= 0f || IsLeavingBounds()) 
        { 
            PickNewDirection(false); 
        } 
 
        ApplyMovement(); 
        AlignRotation(); 
        ClampWaterHeight(); 
    } 
 
    private void ApplyMovement() 
    { 
        Vector3 desiredVelocity = currentDirection * swimSpeed; 
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, turnLerp * Time.fixedDeltaTime); 
    } 
 
    private void AlignRotation() 
    { 
        Vector3 velocity = rb.linearVelocity; 
        if (velocity.sqrMagnitude < 0.0001f) 
            return; 
 
        Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up); 
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnLerp * Time.fixedDeltaTime)); 
    } 
 
    private void PickNewDirection(bool immediate) 
    { 
        Vector3 randomDir = Random.insideUnitSphere; 
        randomDir.y *= verticalBias;
 
        if (randomDir.sqrMagnitude < 0.01f) 
        { 
            randomDir = Vector3.forward; 
        }
        
        // Si está cerca de los límites, dirigir hacia el centro
        if (IsLeavingBounds())
        {
            Vector3 toCenter = (centerPoint - transform.position).normalized;
            randomDir = Vector3.Lerp(randomDir, toCenter, 0.7f);
        }
 
        currentDirection = randomDir.normalized; 
        changeTimer = immediate ? directionChangeInterval * 0.5f : directionChangeInterval * Random.Range(0.7f, 1.3f); 
    } 
 
    private bool IsLeavingBounds() 
    { 
        if (!clampToWater) 
            return false; 
 
        // Comprobar distancia horizontal desde el centro
        Vector3 horizontalPos = transform.position;
        horizontalPos.y = centerPoint.y;
        Vector3 horizontalCenter = centerPoint;
        horizontalCenter.y = centerPoint.y;
        
        float horizontalDistance = Vector3.Distance(horizontalPos, horizontalCenter);
        bool farFromCenter = horizontalDistance > wanderRadius * 0.8f;
 
        // Comprobar límites verticales
        float maxY = waterLevel - surfaceOffset; 
        float minY = waterLevel - waterDepth + bottomOffset; 
        bool outsideHeight = transform.position.y > maxY - 0.5f || transform.position.y < minY + 0.5f; 
 
        return farFromCenter || outsideHeight; 
    } 
 
    private void ClampWaterHeight() 
    { 
        if (!clampToWater) 
            return; 
 
        float maxY = waterLevel - surfaceOffset; 
        float minY = waterLevel - waterDepth + bottomOffset; 
 
        Vector3 pos = transform.position; 
        if (pos.y > maxY || pos.y < minY)
        {
            pos.y = Mathf.Clamp(pos.y, minY, maxY); 
            transform.position = pos;
            
            // Si tocamos límite vertical, cambiar dirección
            if (changeTimer > directionChangeInterval * 0.3f)
            {
                changeTimer = 0.1f;
            }
        }
    }
    
    // Método helper para configurar desde el Inspector
    private void OnDrawGizmosSelected()
    {
        // Visualizar los límites de nado
        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? centerPoint : transform.position;
        Gizmos.DrawWireSphere(center, wanderRadius);
        
        // Visualizar límites de agua
        Gizmos.color = Color.blue;
        float maxY = waterLevel - surfaceOffset;
        float minY = waterLevel - waterDepth + bottomOffset;
        
        Vector3 topCenter = center;
        topCenter.y = maxY;
        Gizmos.DrawWireSphere(topCenter, wanderRadius);
        
        Vector3 bottomCenter = center;
        bottomCenter.y = minY;
        Gizmos.DrawWireSphere(bottomCenter, wanderRadius);
    }
}