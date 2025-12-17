using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterDrag : MonoBehaviour
{
    [Header("Resistencia del Agua")]
    [SerializeField] private float dragCoefficient = 1.5f;
    [SerializeField] private float angularDragCoefficient = 1.0f;
    [SerializeField] private float velocityDragMultiplier = 0.5f;
    
    [Header("Flujo de Agua")]
    [SerializeField] private bool enableWaterFlow = true;
    [SerializeField] private Vector3 flowDirection = Vector3.right;
    [SerializeField] private float flowStrength = 0.5f;
    [SerializeField] private bool randomizeFlow = true;
    [SerializeField] private float flowVariation = 0.2f;
    
    [Header("Turbulencia")]
    [SerializeField] private bool enableTurbulence = true;
    [SerializeField] private float turbulenceStrength = 0.3f;
    [SerializeField] private float turbulenceFrequency = 1f;
    [SerializeField] private Vector3 turbulenceScale = Vector3.one;
    
    [Header("Configuración Específica para Peces")]
    [SerializeField] private bool isFish = true;
    [SerializeField] private float fishDragReduction = 0.7f; // Los peces son más hidrodinámicos
    [SerializeField] private float maxSwimSpeed = 3f;
    [SerializeField] private bool limitSpeed = true;
    
    [Header("Área de Efecto")]
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private bool onlyAffectUnderwater = true;
    
    private Rigidbody rb;
    private Vector3 currentFlowVelocity;
    private float turbulenceTimer = 0f;
    private Vector3 turbulenceOffset;
    
    void Start()
    {
        InitializeDrag();
    }
    
    private void InitializeDrag()
    {
        rb = GetComponent<Rigidbody>();
        
        // Generar offset aleatorio para turbulencia
        turbulenceOffset = new Vector3(
            Random.Range(0f, 100f),
            Random.Range(0f, 100f),
            Random.Range(0f, 100f)
        );
        
        // Configurar flow inicial
        UpdateFlowVelocity();
    }
    
    void FixedUpdate()
    {
        if (onlyAffectUnderwater && transform.position.y > waterLevel)
        {
            return; // No aplicar resistencia fuera del agua
        }
        
        ApplyWaterDrag();
        
        if (enableWaterFlow)
        {
            ApplyWaterFlow();
        }
        
        if (enableTurbulence)
        {
            ApplyTurbulence();
        }
        
        if (limitSpeed)
        {
            LimitVelocity();
        }
    }
    
    private void ApplyWaterDrag()
    {
        // Calcular resistencia basada en velocidad
        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;
        
        if (speed > 0.001f)
        {
            // Fuerza de resistencia cuadrática (realista para fluidos)
            float dragMagnitude = dragCoefficient * speed * speed;
            
            // Reducir drag para peces (más hidrodinámicos)
            if (isFish)
            {
                dragMagnitude *= fishDragReduction;
            }
            
            // Aplicar fuerza de resistencia opuesta a la velocidad
            Vector3 dragForce = -velocity.normalized * dragMagnitude;
            rb.AddForce(dragForce, ForceMode.Force);
        }
        
        // Resistencia angular
        Vector3 angularVelocity = rb.angularVelocity;
        float angularSpeed = angularVelocity.magnitude;
        
        if (angularSpeed > 0.001f)
        {
            float angularDragMagnitude = angularDragCoefficient * angularSpeed;
            Vector3 angularDragTorque = -angularVelocity.normalized * angularDragMagnitude;
            rb.AddTorque(angularDragTorque, ForceMode.Force);
        }
        
        // Resistencia adicional basada en velocidad
        rb.linearVelocity *= (1f - velocityDragMultiplier * Time.fixedDeltaTime);
    }
    
    private void ApplyWaterFlow()
    {
        UpdateFlowVelocity();
        
        // Aplicar fuerza de flujo
        Vector3 flowForce = currentFlowVelocity * flowStrength;
        rb.AddForce(flowForce, ForceMode.Force);
    }
    
    private void UpdateFlowVelocity()
    {
        currentFlowVelocity = flowDirection.normalized;
        
        if (randomizeFlow)
        {
            // Añadir variación al flujo
            float noiseX = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 2f - 1f;
            float noiseY = Mathf.PerlinNoise(Time.time * 0.5f, 100) * 2f - 1f;
            float noiseZ = Mathf.PerlinNoise(Time.time * 0.5f, 200) * 2f - 1f;
            
            Vector3 variation = new Vector3(noiseX, noiseY, noiseZ) * flowVariation;
            currentFlowVelocity += variation;
        }
    }
    
    private void ApplyTurbulence()
    {
        turbulenceTimer += Time.fixedDeltaTime * turbulenceFrequency;
        
        // Generar turbulencia usando Perlin Noise 3D
        Vector3 position = transform.position + turbulenceOffset;
        
        float turbX = (Mathf.PerlinNoise(
            position.x * turbulenceScale.x + turbulenceTimer,
            position.y * turbulenceScale.y
        ) * 2f - 1f) * turbulenceStrength;
        
        float turbY = (Mathf.PerlinNoise(
            position.y * turbulenceScale.y + turbulenceTimer,
            position.z * turbulenceScale.z
        ) * 2f - 1f) * turbulenceStrength;
        
        float turbZ = (Mathf.PerlinNoise(
            position.z * turbulenceScale.z + turbulenceTimer,
            position.x * turbulenceScale.x
        ) * 2f - 1f) * turbulenceStrength;
        
        Vector3 turbulenceForce = new Vector3(turbX, turbY, turbZ);
        rb.AddForce(turbulenceForce, ForceMode.Force);
    }
    
    private void LimitVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSwimSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSwimSpeed;
        }
    }
    
    // Métodos públicos para configuración dinámica
    public void SetDragCoefficient(float coefficient)
    {
        dragCoefficient = coefficient;
    }
    
    public void SetFlowDirection(Vector3 direction)
    {
        flowDirection = direction.normalized;
    }
    
    public void SetFlowStrength(float strength)
    {
        flowStrength = strength;
    }
    
    public void SetTurbulence(bool enabled, float strength = -1f)
    {
        enableTurbulence = enabled;
        if (strength >= 0)
        {
            turbulenceStrength = strength;
        }
    }
    
    public void EnableFishMode(bool enable)
    {
        isFish = enable;
    }
    
    public Vector3 GetCurrentFlowVelocity()
    {
        return currentFlowVelocity * flowStrength;
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        
        // Visualizar dirección del flujo
        if (enableWaterFlow)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, currentFlowVelocity * flowStrength * 2f);
        }
        
        // Visualizar velocidad actual
        if (rb != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, rb.linearVelocity);
        }
    }
}
