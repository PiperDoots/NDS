using System.Collections.Generic;
using UnityEngine;

public class StationarySpawner : MonoBehaviour
{
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Texture2D pattern;

	// Start is called before the first frame update
	void Start()
    {
		SpawnBulletsFromImage();
	}

	private void SpawnBulletsFromImage()
	{

		if (pattern == null || bulletPrefab == null)
		{
			Debug.LogError("PixelImage or bulletPrefab is not assigned!");
			return;
		}

		int width = pattern.width;
		int height = pattern.height;

		Vector3 center = new Vector3(width * 0.5f, height * 0.5f, 0);

		// Create a new empty GameObject to hold all the bullets
		GameObject bulletsContainer = new GameObject(pattern.name)
		{
			tag = "Bullets"
		};
		bulletsContainer.transform.position = transform.position;

		List<GestureManager.GestureTemplate> templates = GestureManager.Instance.templates;
		foreach (GestureManager.GestureTemplate template in templates)
		{
			if (template.Name == pattern.name)
			{
				var bulletRenderer = bulletPrefab.GetComponentInChildren<SpriteRenderer>();
				if (bulletRenderer != null)
				{
					bulletRenderer.color = template.Color;
				}

				var bulletParticles = bulletPrefab.GetComponentInChildren<ParticleSystem>();
				if (bulletParticles != null)
				{
					ParticleSystem.MainModule main = bulletParticles.main;
					ParticleSystem.ColorOverLifetimeModule colorOverLifetime = bulletParticles.colorOverLifetime;

					main.startColor = template.Color;

					// Create a gradient that starts with template.Color and fades to transparent
					Gradient gradient = new Gradient();
					GradientColorKey[] colorKeys = new GradientColorKey[2];
					colorKeys[0].color = template.Color; // Start color
					colorKeys[0].time = 0.0f;
					colorKeys[1].color = new Color(template.Color.r, template.Color.g, template.Color.b, 0.0f); // End color (transparent)
					colorKeys[1].time = 1.0f;
					GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
					alphaKeys[0].alpha = 1.0f; // Start alpha (fully opaque)
					alphaKeys[0].time = 0.0f;
					alphaKeys[1].alpha = 0.0f; // End alpha (fully transparent)
					alphaKeys[1].time = 1.0f;

					gradient.SetKeys(colorKeys, alphaKeys);
					colorOverLifetime.color = gradient;
				}
			}
		}


		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Color pixelColor = pattern.GetPixel(x, y);

				// Check if the pixel is not transparent
				if (pixelColor.a > 0)
				{

					Vector3 spawnPosition = new Vector3(x, y, 0);
					Vector3 direction = (spawnPosition - center).normalized;

					// Define an offset distance from the center in the direction of 'direction'

					// Calculate the new spawn position by adding the offset
					Vector3 offsetSpawnPosition = bulletsContainer.transform.position + direction*.5f;

					float distanceFromCenter = Vector3.Distance(center, spawnPosition);
					float scale = Mathf.Clamp01(distanceFromCenter / (width * 0.5f));
					float bulletSpeed = 0;

					// Create a new bullet object under the bulletsContainer
					GameObject bullet = Instantiate(bulletPrefab, bulletsContainer.transform);
					Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

					bullet.transform.position = offsetSpawnPosition; // Set the modified spawn position

					// Set the bullet's velocity based on the direction and speed
					rb.velocity = direction * bulletSpeed;
				}
			}
		}
	}
}
