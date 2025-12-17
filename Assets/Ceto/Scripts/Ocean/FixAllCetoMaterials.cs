using UnityEngine;
using UnityEditor;

public class FixAllCetoMaterials : EditorWindow
{
    [MenuItem("Ceto/Fix All Materials")]
    static void FixMaterials()
    {
        // Buscar todos los materiales de Ceto
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Ceto/Materials" });

        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null)
            {
                string matName = mat.name.ToLower();
                Shader newShader = null;

                if (matName.Contains("oceantopside") && matName.Contains("opaque"))
                {
                    newShader = Shader.Find("Ceto/OceanTopSide_Opaque");
                }
                else if (matName.Contains("oceantopside") && matName.Contains("transparent"))
                {
                    newShader = Shader.Find("Ceto/OceanTopSide_Transparent");
                }
                else if (matName.Contains("oceanunderside") && matName.Contains("opaque"))
                {
                    newShader = Shader.Find("Ceto/OceanUnderSide_Opaque");
                }
                else if (matName.Contains("oceanunderside") && matName.Contains("transparent"))
                {
                    newShader = Shader.Find("Ceto/OceanUnderSide_Transparent");
                }
                else if (matName.Contains("white"))
                {
                    newShader = Shader.Find("Sprites/Default");
                }

                if (newShader != null)
                {
                    mat.shader = newShader;
                    EditorUtility.SetDirty(mat);
                    Debug.Log($"Fixed material: {mat.name} → {newShader.name}");
                    fixedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Total materials fixed: {fixedCount}");
        EditorUtility.DisplayDialog("Ceto Fix", $"Se repararon {fixedCount} materiales!", "OK");
    }
}