using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DecorationController : MonoBehaviour
{
	[Header("Interacción")]
	public bool enableDrag = true;
	public bool enableDelete = true;

	[Header("UI Reference")]
	public Button deleteButton;

	[Header("Selección")]
	public LayerMask interactableMask = ~0; // Por defecto todas las capas
	public Camera targetCamera; // Si está vacío, usa Camera.main

	private bool dragging = false;
	private Vector3 dragOffset;
	private Plane dragPlane;

	bool selected = false;

	Renderer[] renderers;
	Color[] originalColors;
	Rigidbody rb;

	void Start()
	{
		renderers = GetComponentsInChildren<Renderer>();
		rb = GetComponent<Rigidbody>();
		if (rb != null && enableDrag)
		{
			// Si vamos a mover el objeto por transform, hacerlo kinematic evita
			// conflictos con la física (teleportaciones, fuerzas, etc.).
			rb.isKinematic = true;
		}
		if (renderers != null && renderers.Length > 0)
		{
			originalColors = new Color[renderers.Length];
			for (int i = 0; i < renderers.Length; i++)
			{
				if (renderers[i].material != null && renderers[i].material.HasProperty("_Color"))
					originalColors[i] = renderers[i].material.color;
				else
					originalColors[i] = Color.white;
			}
		}

		// Hide delete button by default until this decoration is selected
		if (deleteButton != null)
		{
			deleteButton.gameObject.SetActive(false);
		}
	}

	void Update()
	{
		HandleInput();
	}
	
	

	#region Funciones Reutilizables de Movimiento

	/// <summary>
	/// Función principal que maneja el input del mouse. Puede ser llamada externamente.
	/// </summary>
	public void HandleInput()
	{
		var cam = targetCamera != null ? targetCamera : Camera.main;
		if (cam == null)
		{
			Debug.LogWarning("DecorationController: No camera found. Make sure Camera.main exists.");
			return;
		}

		var mouse = Mouse.current;
		if (mouse == null) return;

		if (mouse.leftButton.wasPressedThisFrame)
		{
			HandleMouseDown(cam, mouse);
		}

		if (dragging && mouse.leftButton.isPressed)
		{
			HandleDragging(cam, mouse);
		}

		if (dragging && mouse.leftButton.wasReleasedThisFrame)
		{
			HandleMouseUp();
		}
	}

	/// <summary>
	/// Maneja el click inicial del mouse
	/// </summary>
	public void HandleMouseDown(Camera cam, Mouse mouse)
	{
		Vector2 screenPos = mouse.position.ReadValue();
		Ray ray = cam.ScreenPointToRay(screenPos);
		
		int layerToIgnore = LayerMask.NameToLayer("Ignore Raycast");
		interactableMask &= ~(1 << layerToIgnore);

		if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableMask))
		{
			bool isThisObject = hit.collider != null && hit.collider.transform != null && hit.collider.transform.IsChildOf(transform);
			
			if (isThisObject)
			{
				SelectThis();

				if (enableDrag)
				{
					StartDragging(ray);
				}
			}
		}
	}

	/// <summary>
	/// Inicia el arrastre del objeto
	/// </summary>
	public void StartDragging(Ray ray)
	{
		dragPlane = new Plane(Vector3.up, transform.position);
		if (dragPlane.Raycast(ray, out float enter))
		{
			Vector3 hitPoint = ray.GetPoint(enter);
			dragOffset = transform.position - hitPoint;
			dragging = true;
		}
	}

	/// <summary>
	/// Maneja el movimiento mientras se arrastra
	/// </summary>
	public void HandleDragging(Camera cam, Mouse mouse)
	{
		Vector2 screenPos = mouse.position.ReadValue();
		Ray ray = cam.ScreenPointToRay(screenPos);
		
		if (dragPlane.Raycast(ray, out float enter))
		{
			Vector3 hitPoint = ray.GetPoint(enter);
			MoveDecoration(hitPoint + dragOffset);
		}
	}

	/// <summary>
	/// Mueve la decoración a una posición específica
	/// </summary>
	public void MoveDecoration(Vector3 newPosition)
	{
		transform.position = newPosition;
	}

	/// <summary>
	/// Maneja cuando se suelta el mouse
	/// </summary>
	public void HandleMouseUp()
	{
		Debug.Log("Stopped dragging decoration");
		dragging = false;
	}

	#endregion

	void SelectThis()
	{
		selected = true;
		ShowDecorationInfo();
		Highlight(true);
		
		// Notificar al DecorationManager
		if (DecorationManager.Instance != null)
		{
			DecorationManager.Instance.SelectDecoration(this);
		}
	}

	void ShowDecorationInfo()
	{
		Debug.Log("Showing decoration info");

		// Ensure the delete button is visible for this decoration and wired to delete
		if (deleteButton != null)
		{
			deleteButton.gameObject.SetActive(true);
			SetupDeleteButton(deleteButton);
		}
	}

	void OnDisable()
	{
		// Cleanup listeners and hide button when this object is disabled/destroyed
		if (deleteButton != null)
		{
			deleteButton.onClick.RemoveAllListeners();
			if (deleteButton.gameObject != null)
				deleteButton.gameObject.SetActive(false);
		}
	}

	public void SetupDeleteButton(Button deleteButton)
	{
		if (deleteButton == null) return;
		
		deleteButton.onClick.RemoveAllListeners();
		deleteButton.onClick.AddListener(() => {
	        DeleteDecoration();
	  	});
	}

	void DeleteDecoration()
	{
		if (enableDelete && selected)
		{
			Destroy(gameObject);
		}
	}

	void Highlight(bool on)
	{
		if (renderers == null) return;
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] == null) continue;
			if (renderers[i].material != null && renderers[i].material.HasProperty("_Color"))
				renderers[i].material.color = on ? Color.yellow : originalColors[i];
		}
	}
}

