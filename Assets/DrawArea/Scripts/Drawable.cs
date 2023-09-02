using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Drawable : MonoBehaviour
{
	[SerializeField] private Texture2D paintTexture;
	[SerializeField] private Texture2D resetTexture;
	[SerializeField] private Color paintColor = Color.red;
	[SerializeField] private int brushWidth = 10;

	[SerializeField] private List<Vector2> drawpoints = new List<Vector2>();
	private bool isPainting = false;

	private Vector2 previousHitPoint = Vector2.zero;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
		{
			isPainting = true;
		}
		else if (Input.GetMouseButtonUp(0)) // Left mouse button released
		{
			isPainting = false;
			ResetTexture();
			previousHitPoint = Vector2.zero; // Reset the previous hit point
		}

		if (isPainting)
		{
			Vector2 hitPoint = GetHitPoint();

			// If the mouse has moved
			if (hitPoint != Vector2.zero && !drawpoints.Contains(hitPoint))
			{
				if (previousHitPoint != Vector2.zero)
				{
					DrawLine(previousHitPoint, hitPoint);
				}

				PaintTexture(hitPoint);
				drawpoints.Add(hitPoint);

				previousHitPoint = hitPoint;
			}
		}
	}

	private void DrawLine(Vector2 start, Vector2 end)
	{
		Vector2 direction = (end - start).normalized;
		float distance = Vector2.Distance(start, end);

		for (float i = 0; i < distance; i += 1f)
		{
			Vector2 point = start + direction * i;
			PaintTexture(point);
		}
	}

	private Vector2 GetHitPoint()
	{
		Vector2 hitPoint = Vector2.zero;
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

		if (hit.collider != null && hit.collider.gameObject == gameObject)
		{
			Vector2 localHitPoint = hit.point - (Vector2)hit.collider.bounds.min;
			hitPoint = new Vector2(localHitPoint.x / hit.collider.bounds.size.x, localHitPoint.y / hit.collider.bounds.size.y);
			hitPoint.x *= paintTexture.width;
			hitPoint.y *= paintTexture.height;
		}
		return hitPoint;
	}

	public void PaintTexture(Vector2 hitPoint)
	{
		int centerX = Mathf.FloorToInt(hitPoint.x);
		int centerY = Mathf.FloorToInt(hitPoint.y);

		int radius = brushWidth / 2;
		int radiusSquared = radius * radius;

		for (int x = centerX - radius; x <= centerX + radius; x++)
		{
			for (int y = centerY - radius; y <= centerY + radius; y++)
			{
				if (x >= 0 && x < paintTexture.width && y >= 0 && y < paintTexture.height)
				{
					int distanceSquared = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);
					if (distanceSquared <= radiusSquared)
					{
						paintTexture.SetPixel(x, y, paintColor);
					}
				}
			}
		}
		paintTexture.Apply();
	}

	private void ResetTexture()
	{
		GestureManager.Instance.Process(drawpoints);
		drawpoints.Clear();
		Color[] referencePixels = resetTexture.GetPixels();
		paintTexture.SetPixels(referencePixels);
		paintTexture.Apply();
	}
}
