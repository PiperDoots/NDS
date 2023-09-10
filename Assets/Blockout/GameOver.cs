using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{

	[SerializeField] private float lifetime = 10f;
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
			SceneManager.LoadScene("Menu");		
		}
	}
}
