using UnityEngine;
using System.Collections.Generic;

public class BulletSpawner : MonoBehaviour
{
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private float maxBulletSpeed = 5f;
	[SerializeField] private List<Texture2D> patternImages;
	[SerializeField] private bool mirror = false;
	[SerializeField] private float spawnTime = 2;

	private void Start()
	{
		if (mirror)
		{
			SpawnBulletsFromImage();
		}
		else
		{
			Invoke("SpawnBulletsFromImage", spawnTime * .5f);
		}
	}

	private void SpawnBulletsFromImage()
	{
		Texture2D pattern = patternImages[Random.Range(0, patternImages.Count)];

		if (pattern == null || bulletPrefab == null)
		{
			Debug.LogError("PixelImage or BulletPrefab is not assigned!");
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

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Color pixelColor = pattern.GetPixel(x, y);

				// Check if the pixel is not transparent
				if (pixelColor.a > 0)
				{
					int yPosition = mirror ? height - y - 1 : y;

					Vector3 spawnPosition = new Vector3(x, yPosition, 0);
					Vector3 direction = (spawnPosition - center).normalized;

					float distanceFromCenter = Vector3.Distance(center, spawnPosition);
					float scale = Mathf.Clamp01(distanceFromCenter / (width * 0.5f));
					float bulletSpeed = maxBulletSpeed * scale;

					// Create a new bullet object under the bulletsContainer
					GameObject bullet = Instantiate(bulletPrefab, bulletsContainer.transform);
					Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

					bullet.transform.position = transform.position;

					// Set the bullet's velocity based on the direction and speed
					rb.velocity = direction * bulletSpeed;
				}
			}
		}
		Invoke("SpawnBulletsFromImage", spawnTime);
	}
}