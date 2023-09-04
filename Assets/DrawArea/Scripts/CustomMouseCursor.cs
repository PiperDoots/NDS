using UnityEngine;

public class CustomMouseCursor : MonoBehaviour
{
	[SerializeField] private Texture2D mouseCursor;
	[SerializeField] private Texture2D mouseCursorOut;
	private BoxCollider2D customCursorArea; // Reference to the BoxCollider2D component

	Vector2 hotSpot = new Vector2(7, 24);
	CursorMode cursorMode = CursorMode.Auto;

	private void Start()
	{
		Cursor.SetCursor(mouseCursor, hotSpot, cursorMode);

		// Get the BoxCollider2D component attached to this GameObject
		customCursorArea = GetComponent<BoxCollider2D>();

		if (customCursorArea == null)
		{
			Debug.LogError("CustomMouseCursor: BoxCollider2D component not found!");
		}
	}

	private void Update()
	{
		if (customCursorArea == null)
		{
			return; // Don't proceed if the BoxCollider2D component is not found
		}

		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mousePosition.z = 0;
		bool isMouseInsideCollider = customCursorArea.bounds.Contains(mousePosition);

		if (isMouseInsideCollider)
		{
			Cursor.SetCursor(mouseCursor, hotSpot, cursorMode);
		}
		else
		{
			// Set the default cursor outside of the BoxCollider2D bounds
			Cursor.SetCursor(mouseCursorOut, hotSpot, cursorMode);
		}
	}
}
