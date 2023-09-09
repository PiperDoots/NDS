using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JSONPointData
{
	public List<Vector2> points;
}
public class DrawPointsFromJSON : MonoBehaviour
{
	public List<TextAsset> jsonFiles;
	public Drawable drawableComponent;
	private int currentIndex = 0;

	private void Start()
	{
		// Load all TextAssets from the "GestureTemplates" folder under "Resources"
		TextAsset[] loadedJsonFiles = Resources.LoadAll<TextAsset>("GestureTemplates");
		jsonFiles = loadedJsonFiles.ToList();

		LoadPointsFromJSON(currentIndex);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			SwitchJSONFile(-1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			SwitchJSONFile(1);
		}
	}

	private void SwitchJSONFile(int offset)
	{
		int newIndex = currentIndex + offset;
		if (newIndex >= 0 && newIndex < jsonFiles.Count)
		{
			drawableComponent.ResetTexture(); // Clear existing points
			LoadPointsFromJSON(newIndex);
			currentIndex = newIndex;
			Debug.Log("Switched to JSON file: " + jsonFiles[currentIndex].name); // Print the current file name
		}
	}

	private void LoadPointsFromJSON(int index)
	{
		JSONPointData pointData = JsonUtility.FromJson<JSONPointData>(jsonFiles[index].text);

		if (drawableComponent != null)
		{
			foreach (Vector2 point in pointData.points)
			{
				Vector2 newPoint = point;
				newPoint += new Vector2(192 / 2, 256 / 2);
				drawableComponent.PaintTexture(newPoint);
			}
		}
	}
}
