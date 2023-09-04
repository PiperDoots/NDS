using UnityEngine;

public class BulletContainerManager : MonoBehaviour
{
	private BoxCollider2D boundaryCollider;

	private void Start()
	{
		// Try to find a BoxCollider2D component on the same GameObject
		boundaryCollider = GetComponent<BoxCollider2D>();
	}

	private void Update()
	{
		// Find all GameObjects with the "Bullets" tag (assuming you tagged your containers with "Bullets")
		GameObject[] bulletContainers = GameObject.FindGameObjectsWithTag("Bullets");

		foreach (GameObject container in bulletContainers)
		{
			// Check if all bullets within this container have left the boundary collider
			if (AllBulletsLeftBoundary(container))
			{
				// All bullets have left the area, destroy the container
				Destroy(container);
				VariableManager.Instance.LoseLife();
			}
		}
	}

	private bool AllBulletsLeftBoundary(GameObject container)
	{
		if (boundaryCollider == null)
		{
			return false;
		}

		bool allBulletsLeftBoundary = true; // Assume all bullets have left the boundary

		foreach (Transform bullet in container.transform)
		{
			// Check if the bullet's position is inside the boundary collider
			if (boundaryCollider.bounds.Contains(bullet.position))
			{
				// If any bullet is still inside the boundary, set the flag to false
				allBulletsLeftBoundary = false;
				break; 
			}
		}

		return allBulletsLeftBoundary;
	}
}
