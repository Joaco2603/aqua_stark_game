using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterBuoyancy : MonoBehaviour
{
    [Header("Configuración de Agua")]
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private float waterDensity = 1000f;
    [SerializeField] private float gravityInWater = -2f; // Gravedad reducida para pecera
    
    [Header("Propiedades del Objeto")]
    [SerializeField] private float objectDensity = 500f; // Densidad menor que el agua = flota
    [SerializeField] private float buoyancyForceMultiplier = 2f;
    [SerializeField] private bool useAutoVolume = true;
    [SerializeField] private float manualVolume = 1f;
    
    [Header("Amortiguación")]
    [SerializeField] private float waterLinearDrag = 2f;
    [SerializeField] private float waterAngularDrag = 1f;
    [SerializeField] private float airLinearDrag = 0.1f;
    [SerializeField] private float airAngularDrag = 0.05f;
    
    [Header("Efectos de Superficie")]
    [SerializeField] private bool enableSplash = true;
    [SerializeField] private float splashThreshold = 2f;
    [SerializeField] private ParticleSystem splashEffect;
    
    private Rigidbody rb;
    private float objectVolume;
    private bool isUnderwater = false;
    private float previousHeight;
    private Vector3 originalGravity;
    
    void Start()
    {
        InitializeBuoyancy();
    }
    
    private void InitializeBuoyancy()
    {
        rb = GetComponent<Rigidbody>();
        originalGravity = Physics.gravity;
        
        // Calcular volumen del objeto
        if (useAutoVolume)
        {
            CalculateVolume();
        }
        else
        {
            objectVolume = manualVolume;
        }
        
        previousHeight = transform.position.y;
        
        // Configurar Rigidbody para comportamiento en agua
        rb.useGravity = false; // Usaremos gravedad personalizada
    }
    
    private void CalculateVolume()
    {
        // Aproximar volumen usando el bounds del objeto
        Bounds bounds = GetTotalBounds();
        objectVolume = bounds.size.x * bounds.size.y * bounds.size.z;
    }
    
    private Bounds GetTotalBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(transform.position, Vector3.one);
        }
        
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
    
    void FixedUpdate()
    {
        ApplyBuoyancy();
        UpdateDrag();
        CheckSplash();
    }
    
    private void ApplyBuoyancy()
    {
        float objectHeight = transform.position.y;
        
        // Verificar si está bajo el agua
        if (objectHeight < waterLevel)
        {
            isUnderwater = true;
            
            // Calcular profundidad de inmersión
            float submersionDepth = waterLevel - objectHeight;
            float submersionFactor = Mathf.Clamp01(submersionDepth / GetTotalBounds().size.y);
            
            // Fuerza de flotación (Principio de Arquímedes)
            float buoyancyForceMagnitude = waterDensity * objectVolume * Mathf.Abs(gravityInWater) * submersionFactor;
            Vector3 buoyancyForce = Vector3.up * buoyancyForceMagnitude * buoyancyForceMultiplier;
            
            // Aplicar fuerza de flotación
            rb.AddForce(buoyancyForce, ForceMode.Force);
            
            // Aplicar gravedad reducida en agua
            rb.AddForce(Vector3.up * gravityInWater * rb.mass, ForceMode.Acceleration);
        }
        else
        {
            isUnderwater = false;
            
            // Aplicar gravedad normal cuando está fuera del agua
            rb.AddForce(originalGravity * rb.mass, ForceMode.Force);
        }
    }
    
    private void UpdateDrag()
    {
        if (isUnderwater)
        {
            rb.linearDamping = waterLinearDrag;
            rb.angularDamping = waterAngularDrag;
        }
        else
        {
            rb.linearDamping = airLinearDrag;
            rb.angularDamping = airAngularDrag;
        }
    }
    
    private void CheckSplash()
    {
        if (!enableSplash || splashEffect == null)
            return;
        
        float currentHeight = transform.position.y;
        float velocity = (currentHeight - previousHeight) / Time.fixedDeltaTime;
        
        // Detectar entrada al agua
        if (previousHeight > waterLevel && currentHeight <= waterLevel)
        {
            if (Mathf.Abs(velocity) > splashThreshold)
            {
                CreateSplash();
            }
        }
        
        previousHeight = currentHeight;
    }
    
    private void CreateSplash()
    {
        if (splashEffect != null)
        {
            Vector3 splashPosition = new Vector3(transform.position.x, waterLevel, transform.position.z);
            ParticleSystem splash = Instantiate(splashEffect, splashPosition, Quaternion.identity);
            Destroy(splash.gameObject, 2f);
        }
    }
    
    // Métodos públicos para configuración dinámica
    public void SetWaterLevel(float level)
    {
        waterLevel = level;
    }
    
    public void SetObjectDensity(float density)
    {
        objectDensity = density;
    }
    
    public bool IsUnderwater()
    {
        return isUnderwater;
    }
    
    public float GetSubmersionPercentage()
    {
        if (!isUnderwater) return 0f;
        
        float depth = waterLevel - transform.position.y;
        float objectHeight = GetTotalBounds().size.y;
        return Mathf.Clamp01(depth / objectHeight);
    }
    
    private void OnDrawGizmos()
    {
        // Dibujar nivel del agua
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(new Vector3(0, waterLevel, 0), new Vector3(100, 0.1f, 100));
        
        // Dibujar indicador de flotabilidad
        if (Application.isPlaying)
        {
            Gizmos.color = isUnderwater ? Color.cyan : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
