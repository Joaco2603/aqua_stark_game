using UnityEngine;

public class BlenderWaterAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationName = "PlaneAction"; // ← Tu animación
    [SerializeField] private float animationSpeed = 1.0f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool debugMode = true;

    [Header("Material (Opcional)")]
    [SerializeField] private Material waterMaterial;
    [SerializeField] private bool useMaterialShader = false;

    private Animation legacyAnimation;

    private void Start()
    {
        // Intentar primero con Animation (Legacy)
        legacyAnimation = GetComponent<Animation>();

        if (legacyAnimation != null)
        {
            if (debugMode)
            {
                Debug.Log("Usando Animation (Legacy)");

                // Mostrar todas las animaciones disponibles
                foreach (AnimationState state in legacyAnimation)
                {
                    Debug.Log($"Animación encontrada: {state.name}");
                }
            }

            // Configurar y reproducir
            if (legacyAnimation[animationName] != null)
            {
                legacyAnimation[animationName].speed = animationSpeed;
                legacyAnimation[animationName].wrapMode = WrapMode.Loop;

                if (playOnStart)
                {
                    legacyAnimation.Play(animationName);
                    Debug.Log($"✓ Reproduciendo: {animationName}");
                }
            }
            else
            {
                Debug.LogError($"No se encontró la animación '{animationName}'. Revisa el nombre.");
            }
        }
        else
        {
            // Intentar con Animator (Mecanim)
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator != null)
            {
                if (debugMode)
                {
                    Debug.Log("Usando Animator (Mecanim)");
                }

                animator.speed = animationSpeed;

                if (playOnStart)
                {
                    animator.Play(animationName, 0, 0f);
                    Debug.Log($"✓ Reproduciendo: {animationName}");
                }
            }
            else
            {
                Debug.LogError("No se encontró ni Animation ni Animator. Verifica la importación del FBX.");
            }
        }

        // Aplicar material si está configurado
        if (waterMaterial != null && useMaterialShader)
        {
            ApplyWaterMaterial();
        }
    }

    public void PlayWaterAnimation()
    {
        if (legacyAnimation != null)
        {
            legacyAnimation.Play(animationName);
        }
        else if (animator != null)
        {
            animator.Play(animationName, 0, 0f);
        }
    }

    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Clamp(speed, 0.1f, 5f);

        if (legacyAnimation != null && legacyAnimation[animationName] != null)
        {
            legacyAnimation[animationName].speed = animationSpeed;
        }
        else if (animator != null)
        {
            animator.speed = animationSpeed;
        }
    }

    private void ApplyWaterMaterial()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = waterMaterial;
        }

        SkinnedMeshRenderer skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMesh != null)
        {
            skinnedMesh.material = waterMaterial;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SetAnimationSpeed(animationSpeed);
        }
    }
}