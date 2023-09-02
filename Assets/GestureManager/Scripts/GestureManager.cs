using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;


public class GestureManager : MonoBehaviour
{
	[HideInInspector]
	public bool saveAsTemplate = false;
	[HideInInspector]
	public string templateName;

	[SerializeField]
	private List<GestureTemplate> templates = new List<GestureTemplate>();


	[Serializable]
	public struct GestureTemplate
	{
		public string Name;
		public List<Vector2[]> Points;

		public GestureTemplate(string templateName, Vector2[] preparePoints)
		{
			Name = templateName;
			Points = new List<Vector2[]>
			{
				preparePoints
			};
		}
	}
	private class TemplatesData
	{
		public string name;
		public Vector2[] points;
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
		Debug.Log(templatesFilePath);
		templates = LoadTemplates();
	}
	
	public void Process(List<Vector2> drawpoints)
	{
		Vector2[] drawPoints = drawpoints.ToArray();
		Vector2[] processedPoints = drawpoints.ToArray();
		processedPoints = Dollar1Recogniser.Instance.Normalize(processedPoints, 64);

		if (!saveAsTemplate)
		{
			(string bestTemplateName, float bestScore) = Dollar1Recogniser.Instance.DoRecognition(drawPoints, 64, templates);
			Debug.Log("Best Template: " + bestTemplateName + ", Score: " + bestScore);
			
			if (bestTemplateName == "AmongUs")
			{
                UnityEngine.Color color = UnityEngine.Color.red;
				string message = "Sussy Baka";

				Debug.Log(string.Format("<size=24><color=#{0:X2}{1:X2}{2:X2}>{3}</color></size>", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f), message));
				Application.Quit();
				UnityEditor.EditorApplication.isPlaying = false;
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
				GestureTemplate newTemplate = new GestureTemplate(templateName, processedPoints);
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

			foreach (GestureTemplate existingTemplate in loadedTemplates)
			{
				if (existingTemplate.Name == data.name)
				{
					existingTemplate.Points.Add(data.points);
					foundExistingTemplate = true;
					break;
				}
			}

			if (!foundExistingTemplate)
			{
				GestureTemplate newTemplate = new GestureTemplate();
				newTemplate.Name = data.name;
				newTemplate.Points = new List<Vector2[]>();
				newTemplate.Points.Add(data.points);
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
				string json = JsonUtility.ToJson(data, true);

				// Create a unique file name for each template
				string templatesFolderPath = "Assets/GestureTemplates";
				string templateFileName = $"{data.name}_{i}.json";
				string templateFilePath = Path.Combine(templatesFolderPath, templateFileName);

				// Only save the template if the file doesn't exist already
				if (!File.Exists(templateFilePath))
				{
					File.WriteAllText(templateFilePath, json);
				}
				i++;
			}
		}
		Debug.Log("Templates saved.");
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
		}
	}
}
#endif