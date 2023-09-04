using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
	[SerializeField] private float lifetime = 10f;
	[SerializeField] private GameObject spawnPrefab;
	private float timer;

	// Start is called before the first frame update
	void Start()
	{
		timer = 0f;
	}

	// Update is called once per frame
	void Update()
	{
		// Increment the timer
		timer += Time.deltaTime;

		// Check if the object's lifetime has expired
		if (timer >= lifetime)
		{
			Kill();
		}
	}
	public void Kill()
	{
		if (spawnPrefab != null)
			SpawnPrefab();
		Destroy(gameObject);
	}
	// Instantiate the prefab when the object's lifetime expires
	void SpawnPrefab()
	{
		if (spawnPrefab != null)
		{
			Instantiate(spawnPrefab, transform.position, transform.rotation);
		}
	}
}
