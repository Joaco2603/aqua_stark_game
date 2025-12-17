using UnityEngine;

public class CausticsofLight : MonoBehaviour
{
    [Header("Configuración de Proyector")]
    [SerializeField] private Light lightSource;
    [SerializeField] private Projector causticsProjector;
    
    [Header("Texturas de Cáusticas")]
    [SerializeField] private Texture2D[] causticsTextures;
    [SerializeField] private Material causticsMaterial;
    
    [Header("Animación")]
    [SerializeField] private float animationSpeed = 0.5f;
    [SerializeField] private float intensityMultiplier = 1.5f;
    [SerializeField] private Color causticsColor = new Color(0.5f, 0.8f, 1f, 1f);
    
    [Header("Movimiento")]
    [SerializeField] private bool enableWave = true;
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float waveAmplitude = 0.2f;
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.1f, 0.1f);
    
    private int currentTextureIndex = 0;
    private float animationTimer = 0f;
    private float waveTimer = 0f;
    private Vector2 textureOffset = Vector2.zero;
    private Material instanceMaterial;
    
    void Start()
    {
        InitializeCaustics();
    }
    
    private void InitializeCaustics()
    {
        // Crear proyector si no existe
        if (causticsProjector == null)
        {
            GameObject projectorObj = new GameObject("CausticsProjector");
            projectorObj.transform.SetParent(transform);
            projectorObj.transform.localPosition = Vector3.zero;
            projectorObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            causticsProjector = projectorObj.AddComponent<Projector>();
            causticsProjector.orthographic = true;
            causticsProjector.orthographicSize = 5f;
            causticsProjector.nearClipPlane = 0.1f;
            causticsProjector.farClipPlane = 10f;
        }
        
        // Crear material instanciado
        if (causticsMaterial != null)
        {
            instanceMaterial = new Material(causticsMaterial);
            causticsProjector.material = instanceMaterial;
            instanceMaterial.color = causticsColor;
        }
        
        // Aplicar primera textura si existe
        if (causticsTextures != null && causticsTextures.Length > 0)
        {
            UpdateCausticsTexture();
        }
        
        // Configurar luz si existe
        if (lightSource != null)
        {
            lightSource.color = new Color(0.8f, 0.9f, 1f);
            lightSource.intensity = intensityMultiplier;
        }
    }
    
    void Update()
    {
        AnimateCaustics();
        
        if (enableWave)
        {
            SimulateWaveMovement();
        }
        
        UpdateTextureScroll();
    }
    
    private void AnimateCaustics()
    {
        if (causticsTextures == null || causticsTextures.Length == 0)
            return;
        
        animationTimer += Time.deltaTime * animationSpeed;
        
        if (animationTimer >= 1f / animationSpeed)
        {
            animationTimer = 0f;
            currentTextureIndex = (currentTextureIndex + 1) % causticsTextures.Length;
            UpdateCausticsTexture();
        }
    }
    
    private void UpdateCausticsTexture()
    {
        if (instanceMaterial != null && causticsTextures[currentTextureIndex] != null)
        {
            instanceMaterial.SetTexture("_MainTex", causticsTextures[currentTextureIndex]);
        }
    }
    
    private void SimulateWaveMovement()
    {
        waveTimer += Time.deltaTime * waveSpeed;
        
        // Oscilación suave de intensidad
        float intensityWave = 1f + Mathf.Sin(waveTimer) * waveAmplitude;
        
        if (lightSource != null)
        {
            lightSource.intensity = intensityMultiplier * intensityWave;
        }
        
        if (instanceMaterial != null)
        {
            Color currentColor = causticsColor;
            currentColor.a = Mathf.Lerp(0.5f, 1f, (Mathf.Sin(waveTimer) + 1f) * 0.5f);
            instanceMaterial.color = currentColor;
        }
    }
    
    private void UpdateTextureScroll()
    {
        if (instanceMaterial == null)
            return;
        
        textureOffset += scrollSpeed * Time.deltaTime;
        
        // Mantener offset en rango 0-1
        textureOffset.x = textureOffset.x % 1f;
        textureOffset.y = textureOffset.y % 1f;
        
        instanceMaterial.SetTextureOffset("_MainTex", textureOffset);
    }
    
    // Método público para cambiar intensidad
    public void SetIntensity(float intensity)
    {
        intensityMultiplier = intensity;
        if (lightSource != null)
        {
            lightSource.intensity = intensity;
        }
    }
    
    // Método público para cambiar color
    public void SetColor(Color color)
    {
        causticsColor = color;
        if (instanceMaterial != null)
        {
            instanceMaterial.color = color;
        }
    }
    
    private void OnDestroy()
    {
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}
