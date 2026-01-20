using UnityEngine; 
using Fish.Entities;

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

    [Header("Feeding")]
    [SerializeField] private float feedStopDistance = 1.0f; // Distancia aumentada para comer
    [SerializeField] private float feedSpeedMultiplier = 1.4f;
    [SerializeField] private float feedSurfaceHoldOffset = 0.35f;
    [SerializeField] private float feedTurnLerp = 6f;
    [SerializeField] private string foodTag = "Food";
    [SerializeField] private Transform defaultFoodTarget;
    [SerializeField] private int hungerRestoreAmount = 10;
    [SerializeField] private float timeToEatFood = 2f; // Tiempo en segundos para comer la comida
    [SerializeField] private float foodDetectionRadius = 10f; // Radio aumentado para detectar comida cercana

    private Rigidbody rb; 
    private Vector3 initialPosition; 
    private Vector3 centerPoint; 
    private Vector3 currentDirection; 
    private float changeTimer; 
    private bool feeding;
    private Transform foodTarget;
    private float feedingTimer; // Timer para controlar cuándo come la comida
    private bool isCloseToFood; // Flag para saber si está cerca de la comida
    private bool hasBeenEnabledBefore; // Flag para evitar reset en primera habilitación
 
    private void Awake() 
    { 
        rb = GetComponent<Rigidbody>();
        
        // IMPORTANTE: Desactivar gravedad
        rb.useGravity = false;
        rb.linearDamping = 1f; // Añade un poco de resistencia
        
        // Asegurar valores mínimos para detección de comida
        if (feedStopDistance < 1.5f)
            feedStopDistance = 1.5f;
        if (foodDetectionRadius < 10f)
            foodDetectionRadius = 10f;
    }
    
    private void Start()
    {
        // Guardar posición inicial en Start() para que Reproduce.cs pueda establecerla primero
        initialPosition = transform.position; 
        centerPoint = transform.position; 
        PickNewDirection(true);
        Debug.Log($"FishMove Start: initialPosition = {initialPosition}");
    }
 
    private void OnEnable() 
    { 
        // Solo resetear si ya fue habilitado antes (no en el spawn inicial)
        if (resetOnEnable && hasBeenEnabledBefore) 
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
        hasBeenEnabledBefore = true;
    }

    private void FixedUpdate()
    {
        // Estado 1: Alimentándose
        if (feeding)
        {
            HandleFeeding();
            AlignRotation(true);
            ClampWaterHeight();
            return; // Detiene la ejecución aquí si está alimentándose
        }

        // Estado 2: Buscar comida cercana automáticamente
        Transform nearbyFood = FindNearestFood();
        if (nearbyFood != null)
        {
            StartFeeding(nearbyFood);
            return;
        }

        // Estado 3: Navegación normal (Wandering)
        UpdateWanderLogic();
    }

    private void UpdateWanderLogic()
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

    private void ApplyMovement(float speedMultiplier = 1f) 
    {
        //Vector3 desiredVelocity = currentDirection * swimSpeed * speedMultiplier; 
        //rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, turnLerp * Time.fixedDeltaTime);
        Vector3 desiredVelocity = currentDirection * swimSpeed * speedMultiplier;

        // Si está alimentándose, queremos que responda mucho más rápido (usamos un multiplicador de giro)
        float effectiveLerp = feeding ? turnLerp * 2f : turnLerp;

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, effectiveLerp * Time.fixedDeltaTime);
    } 
 
    private void AlignRotation(bool isFeeding = false) 
    { 
        Vector3 velocity = rb.linearVelocity; 
        if (velocity.sqrMagnitude < 0.0001f) 
            return; 
 
        Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up); 
        float lerpSpeed = isFeeding ? feedTurnLerp : turnLerp;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, lerpSpeed * Time.fixedDeltaTime)); 
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


    private void MoveToSurfaceHold()
    {
        float surfaceY = waterLevel - surfaceOffset - feedSurfaceHoldOffset;
        Vector3 holdPos = new Vector3(centerPoint.x, surfaceY, centerPoint.z);
        Vector3 toHold = holdPos - transform.position;

        if (toHold.sqrMagnitude < 0.05f)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, turnLerp * Time.fixedDeltaTime);
            return;
        }

        currentDirection = toHold.normalized;
        ApplyMovement();
    }

    private Transform FindFoodTarget()
    {
        if (defaultFoodTarget != null)
            return defaultFoodTarget;

        GameObject taggedFood = GameObject.FindWithTag(foodTag);
        return taggedFood != null ? taggedFood.transform : null;
    }
    
    // Buscar la comida más cercana dentro del radio de detección
    private Transform FindNearestFood()
    {
        GameObject[] allFood = GameObject.FindGameObjectsWithTag(foodTag);
        
        if (allFood.Length == 0)
            return null;
        
        Transform nearest = null;
        float minDistance = foodDetectionRadius * foodDetectionRadius;
        
        foreach (GameObject food in allFood)
        {
            if (food == null || !food.activeInHierarchy)
                continue;
                
            float distSq = (food.transform.position - transform.position).sqrMagnitude;
            
            if (distSq < minDistance)
            {
                minDistance = distSq;
                nearest = food.transform;
            }
        }
        
        return nearest;
    }

    public void StartFeeding(Transform food = null)
    {
        feeding = true;
        foodTarget = food != null ? food : defaultFoodTarget;
        rb.linearVelocity = Vector3.zero;
        changeTimer = directionChangeInterval * 0.5f;
        feedingTimer = 0f; // Resetear timer
        isCloseToFood = false; // Resetear flag
    }

    public void StopFeeding()
    {
        feeding = false;
        foodTarget = null;
        feedingTimer = 0f;
        isCloseToFood = false;
        PickNewDirection(true);
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

    // Añade estos métodos a tu clase FishMove

    private void OnTriggerEnter(Collider other)
    {
        // 1. Detectar comida por proximidad (El "Ojo" del pez)
        if (other.CompareTag(foodTag) && !feeding)
        {
            StartFeeding(other.transform);
        }
    }

    private void HandleFeeding()
    {
        // Si la comida fue destruida o desactivada
        if (foodTarget == null || !foodTarget.gameObject.activeInHierarchy)
        {
            // Buscar otra comida cercana
            Transform newFood = FindNearestFood();
            if (newFood != null)
            {
                StartFeeding(newFood);
                return;
            }
            
            // No hay más comida, volver a comportamiento normal
            StopFeeding();
            return;
        }

        Vector3 targetPos = foodTarget.position;
        Vector3 toFood = targetPos - transform.position;
        float distance = toFood.magnitude;

        // Si está lo suficientemente cerca
        if (distance <= feedStopDistance)
        {
            // Comenzar a contar el tiempo
            if (!isCloseToFood)
            {
                isCloseToFood = true;
                feedingTimer = 0f;
                Debug.Log("Pez cerca de la comida, comenzando timer...");
            }
            
            feedingTimer += Time.fixedDeltaTime;
            
            // Después de 2 segundos cerca, comer la comida
            if (feedingTimer >= timeToEatFood)
            {
                EatFood();
                return;
            }
            
            // Mientras espera, reducir velocidad y mantenerse cerca
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, turnLerp * Time.fixedDeltaTime);
        }
        else
        {
            // Si se aleja, resetear el timer
            if (isCloseToFood)
            {
                Debug.Log("Pez se alejó de la comida, reseteando timer");
            }
            isCloseToFood = false;
            feedingTimer = 0f;
            
            // Movimiento hacia la comida
            currentDirection = toFood.normalized;
            ApplyMovement(feedSpeedMultiplier);
        }
    }

    private void EatFood()
    {
        if (foodTarget != null)
        {
            // Restaurar hambre si tiene el componente
            FishEntity entity = GetComponent<FishEntity>();
            if (entity != null) entity.hunger += hungerRestoreAmount;
            
            // Destruir la comida
            Destroy(foodTarget.gameObject);
            foodTarget = null;
            
            // Buscar otra comida cercana
            Transform newFood = FindNearestFood();
            if (newFood != null)
            {
                StartFeeding(newFood);
            }
            else
            {
                // No hay más comida, volver a comportamiento normal
                StopFeeding();
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