using UnityEngine;
using UnityEngine.UI;

public class DecorationController : MonoBehaviour
{
	[Header("Interacción")]
	public bool enableDrag = true;
	public bool enableDelete = true;

	bool dragging = false;
	Vector3 dragOffset;
	Plane dragPlane;

	bool selected = false;

	Renderer[] renderers;
	Color[] originalColors;

	void Start()
	{
		renderers = GetComponentsInChildren<Renderer>();
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
	}

	void OnMouseDown()
	{
        ShowDecorationInfo();
		Highlight(true);
		
		if (DecorationManager.Instance != null)
		{
			DecorationManager.Instance.SelectDecoration(this);
		}
	}

    void ShowDecorationInfo()
    {
        UpdateDecoration();
    }

    void UpdateDecoration()
    {
        selected = true;
		if (!enableDrag) return;

		Camera cam = Camera.main;
		if (cam == null) return;

		// Usamos un plano horizontal a la altura del objeto para moverlo en XZ
		dragPlane = new Plane(Vector3.up, transform.position);
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		if (dragPlane.Raycast(ray, out float enter))
		{
			Vector3 hit = ray.GetPoint(enter);
			dragOffset = transform.position - hit;
			dragging = true;
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

	void OnMouseDrag()
	{
		if (!dragging) return;
		Camera cam = Camera.main;
		if (cam == null) return;
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		if (dragPlane.Raycast(ray, out float enter))
		{
			Vector3 hit = ray.GetPoint(enter);
			transform.position = hit + dragOffset;
		}
	}

	void OnMouseUp()
	{
		dragging = false;
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