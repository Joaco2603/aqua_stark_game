using UnityEngine;

public class CetoShaderFix : MonoBehaviour
{
    void Start()
    {
        // Forzar carga de shaders de Ceto
        ForceLoadShader("Ceto/OceanTopSide_Opaque");
        ForceLoadShader("Ceto/OceanTopSide_Transparent");
        ForceLoadShader("Ceto/OceanUnderSide_Opaque");
        ForceLoadShader("Ceto/OceanUnderSide_Transparent");
    }

    void ForceLoadShader(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader != null)
        {
            Debug.Log("Shader encontrado: " + shaderName);
        }
        else
        {
            Debug.LogError("Shader no encontrado: " + shaderName);
        }
    }
}