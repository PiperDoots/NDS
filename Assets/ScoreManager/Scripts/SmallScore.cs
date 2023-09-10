using TMPro;
using UnityEngine;

public class SmallScore : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI smallScoreText;

	private void Start()
	{
		if (smallScoreText != null)
			smallScoreText.text = ScoreManager.Instance.score.ToString();
	}

	private void LateUpdate()
	{
		if (smallScoreText != null)
			smallScoreText.text = ScoreManager.Instance.score.ToString();
	}
}
