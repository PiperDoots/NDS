using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VariableManager : MonoBehaviour
{
	[SerializeField] private AudioSource hitSound;
	public float gameSpeed = 1.0f;
	public int gameLevel = 0;
	[SerializeField] private float maxGameSpeed = 10.0f;
	[SerializeField] private int maxGameLevel = 3;
	[SerializeField] private int maxLives = 5;

	private int lives = 5;
	public int bombs = 2;

	[SerializeField] GameObject fullHeartPrefab;
	[SerializeField] GameObject emptyHeartPrefab;
	[SerializeField] Transform healthIconsParent;

	[SerializeField] Sprite[] bombSprites; 
	[SerializeField] GameObject bombObject;
	private SpriteRenderer bombSpriteRenderer;


	private SongManager songManager;

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

	// Start is called before the first frame update
	void Start()
	{
		songManager = GetComponentInChildren<SongManager>();
		lives = maxLives;
		UpdateLifeCounter();

		bombSpriteRenderer = bombObject.GetComponent<SpriteRenderer>();
	}

	public void LoseLife()
	{
		hitSound.Play();
		lives--;
		gameSpeed = 1.0f;
		songManager.SwitchSong(0);
		if (lives < 0)
		{
			SceneManager.LoadScene("Menu");
		}
		else
		{
			UpdateLifeCounter();
		}
	}

	public void UseBomb()
	{
		bombs--;
		gameSpeed = 1.0f;

		bombSpriteRenderer.sprite = bombSprites[bombs];
	}

	public void ChangeGameSpeed(float multiplier)
	{ 
		gameSpeed *= multiplier;
		gameSpeed = Mathf.Clamp(gameSpeed, gameSpeed, maxGameSpeed);
		if (gameSpeed - 1 < maxGameLevel)
		{
			gameLevel = (int)gameSpeed - 1;
		}
		else
		{
			gameLevel = maxGameLevel;
		}
		
		songManager.SwitchSong(gameLevel);
	}

	private void UpdateLifeCounter()
	{
		float offset = 1f;

		// Clear the existing icons
		foreach (Transform child in healthIconsParent.transform)
		{
			Destroy(child.gameObject);
		}

		// Display full heart objects based on the "lives" value
		for (int i = 0; i < lives; i++)
		{
			var go = Instantiate(fullHeartPrefab, healthIconsParent.transform);
			go.transform.Translate(i * offset, 0, 0);
		}

		// Display empty heart objects based on the remaining lives
		for (int i = 0; i < maxLives - lives; i++)
		{
			var go = Instantiate(emptyHeartPrefab, healthIconsParent.transform);
			go.transform.Translate(i+lives * offset, 0, 0);
		}
	}

}
