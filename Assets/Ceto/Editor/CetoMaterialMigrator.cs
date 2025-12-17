using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Ceto.Migration
{
    /// <summary>
    /// Herramienta para migrar materiales de Ceto de Built-in RP a URP
    /// </summary>
    public class CetoMaterialMigrator : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<Material> materialsToMigrate = new List<Material>();
        private bool scanComplete = false;

        [MenuItem("Tools/Ceto/Migrate Materials to URP")]
        public static void ShowWindow()
        {
            GetWindow<CetoMaterialMigrator>("Ceto URP Migration");
        }

        private void OnEnable()
        {
            ScanForCetoMaterials();
        }

        private void ScanForCetoMaterials()
        {
            materialsToMigrate.Clear();
            
            // Buscar todos los materiales en el proyecto
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                
                if (mat != null && mat.shader != null)
                {
                    string shaderName = mat.shader.name;
                    
                    // Verificar si usa shaders de Ceto Built-in
                    if (shaderName.StartsWith("Ceto/Ocean") && !shaderName.Contains("URP"))
                    {
                        materialsToMigrate.Add(mat);
                    }
                }
            }
            
            scanComplete = true;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Ceto Material Migration Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "Esta herramienta migrará automáticamente tus materiales de Ceto de Built-in RP a URP.\n\n" +
                "IMPORTANTE: Asegúrate de haber habilitado 'Opaque Texture' en tu URP Renderer Asset antes de migrar.",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Rescanear Materiales", GUILayout.Height(30)))
            {
                ScanForCetoMaterials();
            }
            
            EditorGUILayout.Space(10);
            
            if (materialsToMigrate.Count > 0)
            {
                EditorGUILayout.LabelField($"Materiales encontrados: {materialsToMigrate.Count}", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                
                foreach (Material mat in materialsToMigrate)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.ObjectField(mat, typeof(Material), false);
                    EditorGUILayout.LabelField(mat.shader.name, GUILayout.Width(250));
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.Space(10);
                
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("MIGRAR TODOS LOS MATERIALES A URP", GUILayout.Height(40)))
                {
                    MigrateAllMaterials();
                }
                GUI.backgroundColor = Color.white;
            }
            else if (scanComplete)
            {
                EditorGUILayout.HelpBox("No se encontraron materiales de Ceto con shaders Built-in RP.", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);
        }

        private void MigrateAllMaterials()
        {
            if (!EditorUtility.DisplayDialog(
                "Confirmar Migración",
                $"¿Estás seguro de que quieres migrar {materialsToMigrate.Count} materiales a URP?\n\n" +
                "Esta acción se puede deshacer con Ctrl+Z.",
                "Sí, Migrar",
                "Cancelar"))
            {
                return;
            }

            int migratedCount = 0;
            int errorCount = 0;

            foreach (Material mat in materialsToMigrate)
            {
                try
                {
                    Undo.RecordObject(mat, "Migrate Ceto Material to URP");
                    
                    string oldShaderName = mat.shader.name;
                    string newShaderName = GetURPShaderName(oldShaderName);
                    
                    Shader newShader = Shader.Find(newShaderName);
                    
                    if (newShader != null)
                    {
                        mat.shader = newShader;
                        EditorUtility.SetDirty(mat);
                        migratedCount++;
                        Debug.Log($"Migrado: {mat.name} -> {newShaderName}");
                    }
                    else
                    {
                        Debug.LogError($"No se encontró el shader URP: {newShaderName}");
                        errorCount++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error migrando {mat.name}: {e.Message}");
                    errorCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Migración Completada",
                $"Migración completada:\n\n" +
                $"✓ Migrados: {migratedCount}\n" +
                $"✗ Errores: {errorCount}\n\n" +
                $"Verifica la consola para más detalles.",
                "OK"
            );

            ScanForCetoMaterials();
        }

        private string GetURPShaderName(string builtinShaderName)
        {
            // Mapeo de shaders Built-in a URP
            Dictionary<string, string> shaderMap = new Dictionary<string, string>
            {
                { "Ceto/OceanTopSide_Opaque", "Ceto/URP/OceanTopSide_Opaque" },
                { "Ceto/OceanTopSide_Transparent", "Ceto/URP/OceanTopSide_Transparent" },
                { "Ceto/OceanUnderSide_Opaque", "Ceto/URP/OceanUnderSide_Opaque" },
                { "Ceto/OceanUnderSide_Transparent", "Ceto/URP/OceanUnderSide_Transparent" },
                { "Ceto/BlurEffectConeTap", "Ceto/URP/BlurEffectConeTap" }
            };

            if (shaderMap.ContainsKey(builtinShaderName))
            {
                return shaderMap[builtinShaderName];
            }

            // Si no está en el mapeo, intentar agregar /URP/
            return builtinShaderName.Replace("Ceto/", "Ceto/URP/");
        }
    }
}
