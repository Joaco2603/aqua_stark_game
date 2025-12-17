using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BubbleParticles : MonoBehaviour
{
    [Header("Configuración de Burbujas")]
    [SerializeField] private float spawnRate = 10f;
    [SerializeField] private float bubbleSpeed = 0.5f;
    [SerializeField] private float bubbleLifetime = 5f;
    [SerializeField] private Vector2 bubbleSizeRange = new Vector2(0.05f, 0.15f);
    
    [Header("Área de Emisión")]
    [SerializeField] private Vector3 emissionAreaSize = new Vector3(2f, 0.1f, 2f);
    [SerializeField] private bool spawnFromBottom = true;
    
    [Header("Movimiento")]
    [SerializeField] private float wobbleAmount = 0.1f;
    [SerializeField] private float wobbleSpeed = 2f;
    
    private ParticleSystem bubbleSystem;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    private ParticleSystem.SizeOverLifetimeModule sizeModule;
    
    void Start()
    {
        InitializeParticleSystem();
    }
    
    private void InitializeParticleSystem()
    {
        bubbleSystem = GetComponent<ParticleSystem>();
        
        // Configurar módulo principal
        mainModule = bubbleSystem.main;
        mainModule.startLifetime = bubbleLifetime;
        mainModule.startSpeed = bubbleSpeed;
        mainModule.startSize = new ParticleSystem.MinMaxCurve(bubbleSizeRange.x, bubbleSizeRange.y);
        mainModule.gravityModifier = -0.2f; // Gravedad inversa suave para burbujas
        mainModule.maxParticles = 100;
        
        // Configurar emisión
        emissionModule = bubbleSystem.emission;
        emissionModule.rateOverTime = spawnRate;
        
        // Configurar forma de emisión
        shapeModule = bubbleSystem.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Box;
        shapeModule.scale = emissionAreaSize;
        
        // Configurar velocidad con wobble
        velocityModule = bubbleSystem.velocityOverLifetime;
        velocityModule.enabled = true;
        velocityModule.space = ParticleSystemSimulationSpace.Local;
        
        // Movimiento oscilante en X y Z
        AnimationCurve wobbleCurve = AnimationCurve.Linear(0, -1, 1, 1);
        velocityModule.x = new ParticleSystem.MinMaxCurve(wobbleAmount * wobbleSpeed, wobbleCurve);
        velocityModule.z = new ParticleSystem.MinMaxCurve(wobbleAmount * wobbleSpeed, wobbleCurve);
        velocityModule.y = new ParticleSystem.MinMaxCurve(bubbleSpeed);
        
        // Tamaño sobre tiempo (las burbujas crecen ligeramente al subir)
        sizeModule = bubbleSystem.sizeOverLifetime;
        sizeModule.enabled = true;
        AnimationCurve sizeCurve = AnimationCurve.Linear(0, 1, 1, 1.2f);
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
    }
    
    void Update()
    {
        // Actualizar posición si spawn desde el fondo
        if (spawnFromBottom)
        {
            // Las burbujas ya se configuran en Start
        }
    }
    
    // Método para cambiar la tasa de emisión en tiempo de ejecución
    public void SetEmissionRate(float rate)
    {
        spawnRate = rate;
        emissionModule.rateOverTime = rate;
    }
    
    // Método para activar/desactivar burbujas
    public void ToggleBubbles(bool active)
    {
        if (active)
            bubbleSystem.Play();
        else
            bubbleSystem.Stop();
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualizar área de emisión
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireCube(transform.position, emissionAreaSize);
    }
}
