using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowScore : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI scoreText;
	[SerializeField] private TextMeshProUGUI timeText;
	[SerializeField] private TextMeshProUGUI triangleText;
	[SerializeField] private TextMeshProUGUI circleText;
	[SerializeField] private TextMeshProUGUI lineText;
	[SerializeField] private TextMeshProUGUI arrowText;
	[SerializeField] private TextMeshProUGUI totalText;
	[SerializeField] private TextMeshProUGUI mostClearedText;
	[SerializeField] private TextMeshProUGUI highScoreText;

	private void Start()
	{
		if (scoreText != null)
			scoreText.text = "Score: " + ScoreManager.Instance.score;

		if (timeText != null)
		{
			float time = ScoreManager.Instance.time;
			int hours = Mathf.FloorToInt(time / 3600);
			int minutes = Mathf.FloorToInt((time % 3600) / 60);
			int seconds = Mathf.FloorToInt(time % 60);
			int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);
			string timeString = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}", hours, minutes, seconds, milliseconds);
			timeText.text = "Time: " + timeString;
		}

		if (triangleText != null)
			triangleText.text = ": " + ScoreManager.Instance.triangle;

		if (circleText != null)
			circleText.text = ": " + ScoreManager.Instance.circle;

		if (lineText != null)
			lineText.text = ": " + ScoreManager.Instance.line;

		if (arrowText != null)
			arrowText.text = ": " + ScoreManager.Instance.arrow;

		if (totalText != null)
			totalText.text = "Total: " + ScoreManager.Instance.total;

		if (mostClearedText != null)
			mostClearedText.text = "Most: " + ScoreManager.Instance.mostCleared;

		if (highScoreText != null)
			highScoreText.text = ScoreManager.Instance.highScore.ToString();
	}
}
