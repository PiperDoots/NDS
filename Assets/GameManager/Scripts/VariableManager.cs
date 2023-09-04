using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VariableManager : MonoBehaviour
{

	public float gameSpeed = 1.0f;

	[SerializeField] private AudioSource hitSound;

	[SerializeField] private int maxLives = 5;
	private int lives = 5;
	public int bombs = 3;

	// Singleton design pattern, only 1 VariableManager can exist at a time.
	public static VariableManager Instance { get; private set; }
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject);
			Debug.Log("VariableManager already exists");
		}
		else
		{
			Instance = this;
		}
	}

	public void LoseLife()
	{
		hitSound.Play();
		lives--;
		gameSpeed = 1.0f;
	}

	public void UseBomb()
	{
		bombs--;
		gameSpeed = 1.0f;
	}

	// Start is called before the first frame update
	void Start()
    {
		lives = maxLives;
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
