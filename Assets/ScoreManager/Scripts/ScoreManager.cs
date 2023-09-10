using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ScoreManager : MonoBehaviour
{
	public static ScoreManager Instance; // Singleton instance
	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public int score = 0;
	public float time = 0;
	public int mostCleared = 0;
	public int triangle = 0;
	public int circle = 0;
	public int line = 0;
	public int arrow = 0;
	public int total = 0;
	private bool paused = true;

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

	private void Update()
	{
		if (!paused) 
			time += Time.deltaTime;
	}

	public void AddScore(int amount)
	{
		score += amount;
	}
	public void MostCleared(int amount)
	{
		if(amount > mostCleared) 
		{
			mostCleared = amount;
		}
	}
	public void TypeCleared(int amount, string type)
	{
		if (type == "Triangle")
		{
			triangle += amount;
		}
		else if(type == "Circle")
		{
			circle += amount;
		}
		else if (type == "Line")
		{
			line += amount;
		}
		else if (type == "Arrow")
		{
			arrow += amount;
		}
		total += amount;
	}

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		Debug.Log(scene.name);
		if (scene.name == "Menu")
		{
			score = 0;
			mostCleared = 0;
			triangle = 0;
			circle = 0;
			line = 0;
			arrow = 0;
			total = 0;
		}
		else if (scene.name == "BulletScene") 
		{
			paused = false;
		}
		else
		{
			paused = true;
		}
	}
}
