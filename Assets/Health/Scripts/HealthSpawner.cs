using UnityEngine;

public class HealthSpawner : MonoBehaviour
{
	[SerializeField] private GameObject prefabToSpawn; 
	[SerializeField] private Transform spawnArea;
	[SerializeField] private float spawnInterval = 10f; 

	private float timer = 9f;

	private void Update()
	{

		if (VariableManager.Instance.gameLevel > 0)
		{
			timer += Time.deltaTime;
		}

		if (timer >= spawnInterval)
		{
			// Reset the timer
			timer = 0f;

			int randomNumber = Random.Range(0, 1);

			if (randomNumber == 0)
			{
				Vector2 spawnPosition = new Vector2(
					Random.Range(spawnArea.position.x - spawnArea.localScale.x / 2, spawnArea.position.x + spawnArea.localScale.x / 2),
					Random.Range(spawnArea.position.y - spawnArea.localScale.y / 2, spawnArea.position.y + spawnArea.localScale.y / 2)
				);

				Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
			}
		}
	}
}
