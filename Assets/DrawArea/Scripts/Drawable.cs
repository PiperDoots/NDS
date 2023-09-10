using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.tvOS;
using WiimoteApi;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Drawable : MonoBehaviour
{
	[SerializeField] private bool menu = false;
	[SerializeField] private bool gameOver = false;
	[SerializeField] private Texture2D paintTexture;
	[SerializeField] private Texture2D resetTexture;
	[SerializeField] private Texture2D fadingTexture;
	[SerializeField] private Color paintColor = Color.red;
	[SerializeField] private Color finishedColor = Color.red;
	[SerializeField] private int brushWidth = 10;
	[SerializeField] private float timer = 3f;

	[SerializeField] private List<Vector2> drawpoints = new();
	private bool isPainting = false;

	private AudioSource audioSource;
	private Vector2 previousHitPoint = Vector2.zero;

	private bool isFading = false;
	[SerializeField] private float fadeSpeed = 0.02f;

	private Wiimote wiimote;
	private bool prevButtonState = false;
	private bool usingWiiMote = false;
	[SerializeField] private Transform pointerObject;
	[SerializeField] float timeBetweenLEDChanges = 1.0f;
	private bool[] ledStates = new bool[4];

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		audioSource.Play();
		audioSource.Pause();

		InitializeWiimote();
	}

	private void InitializeWiimote()
	{
		WiimoteManager.FindWiimotes(); // Poll native Bluetooth drivers to find Wiimotes

		foreach (Wiimote remote in WiimoteManager.Wiimotes)
		{
			wiimote = remote;
			remote.SendPlayerLED(true, false, false, false);
			remote.SetupIRCamera(IRDataType.EXTENDED);
			Debug.Log("WiiMote Connected");
		}
	}

	public void LedPulse()
	{
		if (wiimote != null)
		{
			StartCoroutine(ChangeLEDs());
		}
	}

	private IEnumerator ChangeLEDs()
	{
		// Set each LED to true one by one
		for (int i = 0; i < ledStates.Length; i++)
		{
			ledStates[i] = true;
			wiimote.SendPlayerLED(ledStates[0], ledStates[1], ledStates[2], ledStates[3]);
			yield return new WaitForSeconds(timeBetweenLEDChanges);

			// Set all LEDs to false
			for (int j = 0; j < ledStates.Length; j++)
			{
				ledStates[j] = false;
			}
			wiimote.SendPlayerLED(ledStates[0], ledStates[1], ledStates[2], ledStates[3]);
			yield return new WaitForSeconds(timeBetweenLEDChanges);
		}
		wiimote.SendPlayerLED(true, false, false, false);
	}


	private void Update()
	{
		if (gameOver)
			timer -= Time.deltaTime;
		if (wiimote != null && !usingWiiMote)
		{
			int ret;
			do
			{
				ret = wiimote.ReadWiimoteData();
				float[] pointer = wiimote.Ir.GetPointingPosition();

				if (ret > 0 && pointer[0] >= 0 && pointer[1] >= 0)
				{
					usingWiiMote = true;
					LedPulse();
					pointerObject.gameObject.GetComponent<SpriteRenderer>().enabled = true;
				}
			} while (ret > 0);
		}
		if (usingWiiMote && wiimote != null)
		{
			int ret;
			do
			{
				ret = wiimote.ReadWiimoteData();
				float[] pointer = wiimote.Ir.GetPointingPosition();
				float[] accel = wiimote.Accel.GetCalibratedAccelData();

				if (ret > 0 && pointer[0] >= 0 && pointer[1] >= 0)
				{
					// Calculate the position in Unity world space based on camera space position
					Vector3 worldPosition = Camera.main.ViewportToWorldPoint(new Vector3(pointer[0], pointer[1], 10)); // You may need to adjust the Z value to match your camera setup

					// Move the targetObject (GameObject) to the calculated position
					pointerObject.position = worldPosition;

					float rotationAngle = Mathf.Atan2(accel[2], accel[0]) * Mathf.Rad2Deg;
					rotationAngle -= 80;
					pointerObject.rotation = Quaternion.Euler(0f, 0f, rotationAngle);
				}
			} while (ret > 0);

			if (wiimote.Button.a && !prevButtonState) // Left mouse button pressed
			{
				isPainting = true;
				audioSource.UnPause();
			}
			else if (!wiimote.Button.a && prevButtonState) // Left mouse button released
			{
				if (menu)
				{
					if (gameOver)
					{
						ResetTexture();
						SceneManager.LoadScene("Menu");
					}
					CreateColliderAroundDrawPoints();
				}
				isPainting = false;
				isFading = true;
				ResetTexture();
				previousHitPoint = Vector2.zero; // Reset the previous hit point
				audioSource.Pause();
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
			if (isFading)
			{
				FadeOutDrawing();
			}

			prevButtonState = wiimote.Button.a;
		}
		else
		{
			if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
			{
				isPainting = true;
				audioSource.UnPause();
			}
			else if (Input.GetMouseButtonUp(0)) // Left mouse button released
			{
				if (menu)
				{
					if (gameOver && timer < 0)
					{
						RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

						if (hit.collider != null && hit.collider.gameObject == gameObject)
						{
							ResetTexture();
							SceneManager.LoadScene("Menu");
						}
					}
					CreateColliderAroundDrawPoints();
				}
				isPainting = false;
				isFading = true;
				ResetTexture();
				previousHitPoint = Vector2.zero; // Reset the previous hit point
				audioSource.Pause();
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
			if (isFading)
			{
				FadeOutDrawing();
			}
		}
	}

	private void FadeOutDrawing()
	{
		Color[] currentPixels = fadingTexture.GetPixels();
		Color[] referencePixels = resetTexture.GetPixels();

		bool isFinishedFading = true;

		for (int i = 0; i < currentPixels.Length; i++)
		{
			currentPixels[i] = Color.Lerp(currentPixels[i], referencePixels[i], fadeSpeed);

			// Check if the pixel has fully faded
			if (currentPixels[i].a > 0.1f)
			{
				isFinishedFading = false;
			}
			else
			{
				// If the pixel has fully faded, set it to fully transparent
				currentPixels[i] = referencePixels[i];
			}
		}

		fadingTexture.SetPixels(currentPixels);
		fadingTexture.Apply();

		if (isFinishedFading)
		{
			isFading = false;
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
		RaycastHit2D hit = new RaycastHit2D();
		if (usingWiiMote)
		{
			if (wiimote != null)
			{
				float[] pointer = wiimote.Ir.GetPointingPosition();
				hit = Physics2D.Raycast(Camera.main.ViewportToWorldPoint(new Vector2(pointer[0], pointer[1])), Vector2.zero);
			}
		}
		else
		{
			hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		}

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

	public void ResetTexture()
	{
		if (!menu)
			GestureManager.Instance.Process(drawpoints);
		drawpoints.Clear();

		if (GestureManager.Instance.templateColor != null)
		{
			finishedColor = GestureManager.Instance.templateColor;
		}
		

		Color[] referencePixels = paintTexture.GetPixels();
		Color[] fadingPixels = fadingTexture.GetPixels();

		const float colorTolerance = 0.1f;
		for (int i = 0; i < referencePixels.Length; i++)
		{
			// Check if the current pixel color matches the paintColor
			float colorDifference = Vector4.Distance(referencePixels[i], paintColor);
			if (colorDifference <= colorTolerance)
			{
				// Replace the fading texture pixel with the second color
				fadingPixels[i] = finishedColor;
			}
			else
			{
				fadingPixels[i] = referencePixels[i];
			}
		}

		// Apply the modified fading texture pixels
		fadingTexture.SetPixels(fadingPixels);
		fadingTexture.Apply();

		Color[] currentPixels = resetTexture.GetPixels();

		paintTexture.SetPixels(currentPixels);
		paintTexture.Apply();
	}

	private void CreateColliderAroundDrawPoints()
	{
		if (drawpoints.ToArray() == null || drawpoints.ToArray().Length == 0)
		{
			return;
		}
		var drawpointArray = Dollar1Recogniser.Instance.Resample(drawpoints.ToArray(), 64);
		var col = GetComponent<BoxCollider2D>();
		Vector2 bottomLeftPoint = new Vector2(col.bounds.min.x, col.bounds.min.y);

		// Create a new empty GameObject to hold the collider
		GameObject colliderObject = new GameObject("DrawCollider");
		colliderObject.transform.position = new Vector3(bottomLeftPoint.x + 0.5f, bottomLeftPoint.y, 0f);
		colliderObject.transform.localScale = new Vector3(1 / 16f, 1 / 16f, 0f);

		// Attach a PolygonCollider2D component to the new GameObject
		PolygonCollider2D collider = colliderObject.AddComponent<PolygonCollider2D>();

		// Convert drawpoints from world space to local space
		List<Vector2> localDrawPoints = new List<Vector2>();
		foreach (Vector2 drawPoint in drawpointArray)
		{
			Vector2 localPoint = transform.InverseTransformPoint(drawPoint);
			localDrawPoints.Add(localPoint);
		}

		// Set the collider points to the local draw points
		collider.SetPath(0, localDrawPoints.ToArray());

		Debug.Log(collider.errorState.ToString());
		if (collider.errorState.ToString() == "None")
		{
			col.enabled = false;

			// Check for GameObjects inside the collider
			Collider2D[] colliders = new Collider2D[1];
			Physics2D.OverlapCollider(collider, new ContactFilter2D(), colliders);

			GameObject menuObject = null;
			bool multipleSelect = false;
			if (colliders.Length > 0)
			{
				foreach (var c in colliders)
				{
					if (c != null && c.CompareTag("Menu"))
					{
						if (menuObject == null)
						{
							menuObject = c.gameObject;
						}
						else
						{
							multipleSelect = true;
							return;
						}
					}
				}
			}
			col.enabled = true;
			if (menuObject != null && !multipleSelect)
			{
				ResetTexture();
				Destroy(colliderObject);
				if (menuObject.name == "Start")
				{
					SceneManager.LoadScene("BulletScene");
				}
				if (menuObject.name == "Quit")
				{
					Application.Quit();
				}
				if (menuObject.name == "Menu")
				{
					SceneManager.LoadScene("Menu");
				}
				if (menuObject.name == "Tutorial")
				{
					SceneManager.LoadScene("Tutorial");
				}
			}
		}
		Destroy(colliderObject);
	}
}
