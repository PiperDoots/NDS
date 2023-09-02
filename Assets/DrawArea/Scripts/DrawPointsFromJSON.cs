using System.Collections.Generic;
using UnityEngine;

public class JSONPointData
{
	public List<Vector2> points;
}

public class DrawPointsFromJSON : MonoBehaviour
{
	public List<TextAsset> jsonFiles;
	public Drawable drawableComponent;

	private JSONPointData pointData;

	private void Start()
	{
		foreach (var file in jsonFiles)
		{
			pointData = JsonUtility.FromJson<JSONPointData>(file.text);

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
}
