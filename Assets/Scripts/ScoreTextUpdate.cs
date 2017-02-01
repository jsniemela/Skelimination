using UnityEngine;
using UnityEngine.UI;

public class ScoreTextUpdate : MonoBehaviour
{
	Text textComponent;
	void Start()
	{
		textComponent = GetComponent<Text>();
	}

	void Update()
	{
		GameManager gameManager = GameManager.GetInstance();
		if (gameManager == null)
			return;

		if (gameManager.Scores.Count > 0)
			textComponent.text = "Score: " + gameManager.GetScoreForPlayer(gameManager.LocalPlayer);
		else
			textComponent.text = "Dead";
	}
}
