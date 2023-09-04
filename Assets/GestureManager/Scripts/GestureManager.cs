using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GestureManager : MonoBehaviour
{
	[HideInInspector]
	public bool saveAsTemplate = false;
	[HideInInspector]
	public string templateName;
	[HideInInspector]
	public Color templateColor = Color.red;

	[SerializeField]
	public List<GestureTemplate> templates = new List<GestureTemplate>();

	[SerializeField]
	private float detectionThreshold = 0.7f;
	[SerializeField]
	private AudioSource hitSound;
	[SerializeField]
	private AudioSource missSound;
	[SerializeField]
	private AudioSource bombSound;

	[Serializable]
	public struct GestureTemplate
	{
		public string Name;
		public Color Color;
		public List<Vector2[]> Points;

		public GestureTemplate(string templateName, Vector2[] preparePoints, Color templateColor)
		{
			Name = templateName;
			Points = new List<Vector2[]>
			{
				preparePoints
			};
			Color = templateColor;
		}
	}
	private class TemplatesData
	{
		public string name;
		public Vector2[] points;
		public Color color;
	}

	private string templatesFilePath;

	// Singleton design pattern, only 1 GestureManager can exist at a time.
	public static GestureManager Instance { get; private set; }
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject);
			Debug.Log("GestureManager already exists");
		}
		else
		{
			Instance = this;
			templatesFilePath = Path.Combine(Application.persistentDataPath, "gesture_templates.json");
			templates = LoadTemplates();
		}
	}

	private void Start()
	{
		templatesFilePath = Path.Combine(Application.persistentDataPath, "gesture_templates.json");
		templates = LoadTemplates();
	}

	public void Process(List<Vector2> drawpoints)
	{
		Vector2[] drawPoints = drawpoints.ToArray();
		Vector2[] processedPoints = drawpoints.ToArray();
		processedPoints = Dollar1Recogniser.Instance.Normalize(processedPoints, 64);

		if (!saveAsTemplate)
		{
			(string bestTemplateName, float bestScore, Color bestColor) = Dollar1Recogniser.Instance.DoRecognition(drawPoints, 64, templates);
			Debug.Log("Best Template: " + bestTemplateName + ", Score: " + bestScore);
			if (bestScore >= detectionThreshold)
			{
				float pitch = 0.5f - detectionThreshold + bestScore;

				GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullets");
				GameObject[] health = GameObject.FindGameObjectsWithTag("Health");
				if (bestTemplateName == "Pentagram" && VariableManager.Instance.bombs > 0)
				{
					PlayRandomPitch(hitSound);
					bombSound.Play();
					templateColor = bestColor;
					VariableManager.Instance.UseBomb();
					foreach (GameObject bullet in bullets)
					{
						Destroy(bullet);
					}
				}
				else if (bestTemplateName == "Pentagram")
				{
					PlayPitch(missSound, 0.5f);
					templateColor = Color.clear;
				}
				else if (bestTemplateName == "Heart" && health.Length > 0)
				{
					PlayRandomPitch(hitSound);
					templateColor = bestColor;
					foreach (GameObject h in health)
					{
						h.GetComponent<Lifetime>().Kill();
					}
					VariableManager.Instance.GainLife();
				}
				else
				{
					templateColor = bestColor;
					bool increaseSpeed = false;
					foreach (GameObject bullet in bullets)
					{
						if (bullet.name == bestTemplateName)
						{
							pitch += 0.5f;
							increaseSpeed = true;
							Destroy(bullet);
						}
					}
					if (increaseSpeed)
					{
						VariableManager.Instance.ChangeGameSpeed(1 + (.2f * bestScore)/((VariableManager.Instance.gameLevel+1)*3));
					}
					PlayPitch(hitSound, pitch);
				}
				if (bestTemplateName == "AmongUs")
				{
					PlayRandomPitch(hitSound);
					templateColor = bestColor;
					Color color = Color.red;
					string message = "Sussy Baka";

					Debug.Log(string.Format("<size=24><color=#{0:X2}{1:X2}{2:X2}>{3}</color></size>", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f), message));
					Application.Quit();
					//UnityEditor.EditorApplication.isPlaying = false;
				}
			}
			else
			{
				PlayRandomPitch(missSound);
				templateColor = Color.clear;
			}
		}
		else
		{
			// Process drawpoints using $1
			bool templateExists = false;
			foreach (GestureTemplate template in templates)
			{
				if (template.Name == templateName)
				{
					template.Points.Add(processedPoints);
					templateExists = true;
					Debug.Log("Added points to existing template: " + templateName);
					break;
				}
			}
			if (!templateExists)
			{
				GestureTemplate newTemplate = new GestureTemplate(templateName, processedPoints, templateColor);
				templates.Add(newTemplate);
				Debug.Log("Created a new template: " + templateName);
			}
		}
	}

	private void OnApplicationQuit()
	{
		SaveTemplates();
	}
	private List<GestureTemplate> LoadTemplates()
	{
		List<GestureTemplate> loadedTemplates = new List<GestureTemplate>();

		string templatesFolderPath = "Assets/GestureTemplates";
		string[] templateFiles = Directory.GetFiles(templatesFolderPath, "*.json");

		foreach (string templateFilePath in templateFiles)
		{
			string json = File.ReadAllText(templateFilePath);
			TemplatesData data = JsonUtility.FromJson<TemplatesData>(json);

			bool foundExistingTemplate = false;
			GestureTemplate existingTemplate = default;

			foreach (GestureTemplate template in loadedTemplates)
			{
				if (template.Name == data.name)
				{
					existingTemplate = template;
					foundExistingTemplate = true;
					break;
				}
			}

			if (foundExistingTemplate)
			{
				existingTemplate.Points.Add(data.points);
				existingTemplate.Color = data.color;
			}
			else
			{
				GestureTemplate newTemplate = new GestureTemplate();
				newTemplate.Name = data.name;
				newTemplate.Points = new List<Vector2[]>();
				newTemplate.Points.Add(data.points);
				newTemplate.Color = data.color;
				loadedTemplates.Add(newTemplate);
			}
		}

		return loadedTemplates;
	}

	private void SaveTemplates()
	{
		foreach (GestureTemplate template in templates)
		{
			int i = 0;
			foreach (Vector2[] points in template.Points)
			{
				TemplatesData data = new TemplatesData();
				data.name = template.Name;
				data.points = points;
				data.color = template.Color;
				string json = JsonUtility.ToJson(data, true);

				// Create a unique file name for each template
				string templatesFolderPath = "Assets/GestureTemplates";
				string templateFileName = $"{data.name}_{i}.json";
				string templateFilePath = Path.Combine(templatesFolderPath, templateFileName);

				// Check if the file already exists
				if (File.Exists(templateFilePath))
				{
					// Read the existing file and check the color
					string existingJson = File.ReadAllText(templateFilePath);
					TemplatesData existingData = JsonUtility.FromJson<TemplatesData>(existingJson);

					// Compare the colors
					if (existingData.color == template.Color)
					{
						// The colors are the same, no need to override
						i++;
						continue;
					}
				}

				// Save the template (override if it exists or create a new file)
				File.WriteAllText(templateFilePath, json);
				i++;
			}
		}
		Debug.Log("Templates saved.");
	}

	public void PlayRandomPitch(AudioSource audioSource)
	{
		float randomPitch = UnityEngine.Random.Range(0.8f, 1.2f);
		audioSource.pitch = randomPitch;
		audioSource.Play();
	}

	public void PlayPitch(AudioSource audioSource, float pitch)
	{
		audioSource.pitch = pitch;
		audioSource.Play();
	}

}
#if UNITY_EDITOR
	[CustomEditor(typeof(GestureManager))]
public class GestureManager_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector(); // for other non-HideInInspector fields

		GestureManager script = (GestureManager)target;

		// draw checkbox for the bool
		script.saveAsTemplate = EditorGUILayout.Toggle("Save gesture as Template", script.saveAsTemplate);
		if (script.saveAsTemplate) // if bool is true, show other fields
		{
			script.templateName = EditorGUILayout.TextField("Template Name", script.templateName);
			script.templateColor = EditorGUILayout.ColorField("Template Color", script.templateColor);
		}
	}
}
#endif
