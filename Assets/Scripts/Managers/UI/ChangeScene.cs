

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility MonoBehaviour to change scenes from UI Buttons.
/// Usage:
/// - Attach this component to any GameObject.
/// - In a Button's OnClick, add the GameObject and select `LoadSceneByName` (string) or `LoadAssignedScene()`.
/// - If using `LoadAssignedScene`, set the `sceneName` field in the inspector.
/// Make sure the target scene is added to Build Settings (File > Build Settings...).
/// </summary>
public class ChangeScene : MonoBehaviour
{
	[Tooltip("Nombre de la escena a cargar cuando se use LoadAssignedScene().")]
	public string sceneName;

	/// <summary>
	/// Llama a SceneManager.LoadScene con el nombre proporcionado.
	/// Conecta este método al `OnClick()` de un Button y pasa el nombre de la escena como parámetro.
	/// </summary>
	/// <param name="name">Nombre de la escena a cargar (debe estar en Build Settings).</param>
	public void LoadSceneByName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			Debug.LogWarning("ChangeScene.LoadSceneByName: nombre de escena vacío o nulo.");
			return;
		}

		SceneManager.LoadScene(name);
	}

	/// <summary>
	/// Carga la escena indicada en el campo `sceneName` del inspector.
	/// Útil si prefieres no pasar el parámetro desde el Button.
	/// </summary>
	public void LoadAssignedScene()
	{
		LoadSceneByName(sceneName);
	}
}

