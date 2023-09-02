using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// $1 is a geometric template matcher used to recognise unistrokes, it's really neat I think !!
// https://depts.washington.edu/acelab/proj/dollar/index.html

public class Dollar1Recogniser : MonoBehaviour
{
	// Singleton design pattern, only 1 Dollar1Recogniser can exist at a time.
	public static Dollar1Recogniser Instance { get; private set; }
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject);
			Debug.Log("Dollar1Recogniser already exists");
		}
		else
		{
			Instance = this;
		}
	}

	public Vector2[] Normalize(Vector2[] points, int targetPointCount)
	{
		Vector2[] copyPoints = new Vector2[points.Length];
		points.CopyTo(copyPoints, 0);
		Vector2[] resampledPoints = Resample(copyPoints, targetPointCount);
		Vector2[] rotatedPoints = RotateToZero(resampledPoints);
		Vector2[] scaledPoints = ScaleToSquare(rotatedPoints, 100);
		Vector2[] translatedToOrigin = TranslateToOrigin(scaledPoints);

		return translatedToOrigin;
	}

	public (string, float) DoRecognition(Vector2[] points, int n, List<GestureManager.GestureTemplate> gestureTemplates)
	{
		Vector2[] preparedPoints = Normalize(points, n);
		return Recognize(preparedPoints, gestureTemplates);
	}

	// Step 1 
	private Vector2[] Resample(Vector2[] points, int targetPointCount)
	{
		// Calculate the desired interval between points
		float interval = PathLength(points) / (targetPointCount - 1);
		float distanceSoFar = 0;
		List<Vector2> newPointsList = new List<Vector2> { points[0] };

		for (int i = 1; i < points.Length; i++)
		{
			float segmentDistance = Vector2.Distance(points[i - 1], points[i]);

			// Check if adding the current segment would exceed the desired interval
			if (distanceSoFar + segmentDistance >= interval)
			{
				// Calculate the position of the new point
				float ratio = (interval - distanceSoFar) / segmentDistance;
				float newX = points[i - 1].x + ratio * (points[i].x - points[i - 1].x);
				float newY = points[i - 1].y + ratio * (points[i].y - points[i - 1].y);
				Vector2 newPoint = new Vector2(newX, newY);

				// Add the new point to the list
				newPointsList.Add(newPoint);

				// Insert the new point into the original points array
				List<Vector2> tempList = new List<Vector2>(points);
				tempList.Insert(i, newPoint);
				points = tempList.ToArray();

				// Reset the distance accumulator
				distanceSoFar = 0;
			}
			else
			{
				// Accumulate distance for segments that don't exceed the interval
				distanceSoFar += segmentDistance;
			}
		}

		return newPointsList.ToArray();
	}
	private float PathLength(Vector2[] path)
	{
		float totalDistance = 0;
		for (int i = 1; i < path.Length; i++)
		{
			// Calculate distance between consecutive points and accumulate
			totalDistance += Vector2.Distance(path[i - 1], path[i]);
		}
		return totalDistance;
	}

	// Step 2
	private Vector2 CalculateCentroid(Vector2[] points)
	{
		Vector2 centroid = Vector2.zero;

		// Calculate the sum of x and y coordinates
		foreach (Vector2 p in points)
		{
			centroid += p;
		}

		// Return Centroid
		return centroid / points.Length;
	}
	private Vector2[] RotateToZero(Vector2[] points)
	{
		// Calculate the centroid of the points
		Vector2 centroid = CalculateCentroid(points);

		// Calculate the angle between the centroid and the first point
		float angleToRotate = Mathf.Atan2(centroid.y - points[0].y, centroid.x - points[0].x);

		// Rotate the points by the negative of the calculated angle
		return RotatePoints(points, -angleToRotate);
	}
	private Vector2[] RotatePoints(Vector2[] points, float angle)
	{
		Vector2[] newPoints = new Vector2[points.Length];

		// Calculate the centroid of the points
		Vector2 centroid = CalculateCentroid(points);

		for (int i = 0; i < points.Length; i++)
		{
			// Apply rotation transformation to each point
			float qx = (points[i].x - centroid.x) * Mathf.Cos(angle) - (points[i].y - centroid.y) * Mathf.Sin(angle) + centroid.x;
			float qy = (points[i].x - centroid.x) * Mathf.Sin(angle) + (points[i].y - centroid.y) * Mathf.Cos(angle) + centroid.y;
			newPoints[i] = new Vector2(qx, qy);
		}

		return newPoints;
	}

	// Step 3
	private Vector2[] ScaleToSquare(Vector2[] points, float targetSize)
	{
		Rect boundingBox = CalculateBoundingBox(points);
		Vector2[] newPoints = new Vector2[points.Length];

		for (int i = 0; i < points.Length; i++)
		{
			// Scale each point based on the bounding box dimensions
			float scaledX = points[i].x * (targetSize / boundingBox.width);
			float scaledY = points[i].y * (targetSize / boundingBox.height);
			newPoints[i] = new Vector2(scaledX, scaledY);
		}

		return newPoints;
	}
	private Rect CalculateBoundingBox(Vector2[] points)
	{
		float minX = Mathf.Infinity, minY = Mathf.Infinity;
		float maxX = Mathf.NegativeInfinity, maxY = Mathf.NegativeInfinity;

		for (int i = 0; i < points.Length; i++)
		{
			// Update min and max values for x and y coordinates
			if (points[i].x < minX) minX = points[i].x;
			if (points[i].x > maxX) maxX = points[i].x;
			if (points[i].y < minY) minY = points[i].y;
			if (points[i].y > maxY) maxY = points[i].y;
		}

		// Create a bounding box rectangle
		return new Rect(minX, minY, maxX - minX, maxY - minY);
	}

	// Step 4
	private Vector2[] TranslateToOrigin(Vector2[] points)
	{
		Vector2 centroid = CalculateCentroid(points);
		Vector2[] newPoints = new Vector2[points.Length];

		for (int i = 0; i < points.Length; i++)
		{
			// Translate each point by subtracting the centroid
			newPoints[i] = points[i] - centroid;
		}
		return newPoints;
	}

	private (string, float) Recognize(Vector2[] inputPoints, List<GestureManager.GestureTemplate> gestureTemplates)
	{
		float bestDistance = Mathf.Infinity;
		Vector2[] bestMatchedTemplate = new Vector2[0];
		float bestScore = 0;
		string bestTemplateName = "None";

		foreach (var template in gestureTemplates)
		{
			foreach (Vector2[] templatePoints in template.Points)
			{
				float distance = DistanceAtBestAngle(inputPoints, templatePoints, -Mathf.PI, Mathf.PI, 2 * Mathf.PI / 180);

				if (distance < bestDistance)
				{
					bestDistance = distance;
					bestMatchedTemplate = templatePoints;
					bestScore = 1 - bestDistance / (0.5f * Mathf.Sqrt(bestMatchedTemplate.Length * bestMatchedTemplate.Length + bestMatchedTemplate.Length * bestMatchedTemplate.Length));
					bestTemplateName = template.Name;
				}
			}
		}

		return (bestTemplateName, bestScore);
	}

	private float DistanceAtBestAngle(Vector2[] inputPoints, Vector2[] templatePoints, float angleA, float angleB, float angleDelta)
	{
		float phi = 0.5f * (-1 + Mathf.Sqrt(5));
		float x1 = phi * angleA + (1 - phi) * angleB;
		float f1 = DistanceAtAngle(inputPoints, templatePoints, x1);
		float x2 = (1 - phi) * angleA + phi * angleB;
		float f2 = DistanceAtAngle(inputPoints, templatePoints, x2);

		while (Mathf.Abs(angleB - angleA) > angleDelta)
		{
			if (f1 < f2)
			{
				angleB = x2;
				x2 = x1;
				f2 = f1;
				x1 = phi * angleA + (1 - phi) * angleB;
				f1 = DistanceAtAngle(inputPoints, templatePoints, x1);
			}
			else
			{
				angleA = x1;
				x1 = x2;
				f1 = f2;
				x2 = (1 - phi) * angleA + phi * angleB;
				f2 = DistanceAtAngle(inputPoints, templatePoints, x2);
			}
		}

		return Mathf.Min(f1, f2);
	}

	private float DistanceAtAngle(Vector2[] inputPoints, Vector2[] templatePoints, float theta)
	{
		Vector2[] rotatedInputPoints = RotatePoints(inputPoints, theta);
		float distance = PathDistance(rotatedInputPoints, templatePoints);
		return distance;
	}

	float PathDistance(Vector2[] A, Vector2[] B)
	{
		int minLength = Mathf.Min(A.Length, B.Length);
		float totalDistance = 0;

		for (int i = 0; i < minLength; i++)
		{
			totalDistance += Vector2.Distance(A[i], B[i]);
		}

		return totalDistance / minLength;
	}

}
