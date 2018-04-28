using UnityEngine;

public class ScoreDisplayControl : MonoBehaviour
{
	private TextMesh scoreText;
	private int score;

	//float timer;

	// Use this for initialization
	void Start ()
	{
		scoreText = GetComponent<Transform>().Find("text_score").GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		/*timer += Time.deltaTime;

		if(timer > 1)
		{
			timer--;
			AddScore(1);
		}*/
	}

	public void SetScore(int score)
	{
		this.score = score;
		UpdateText();
	}

	public void AddScore(int score)
	{
		this.score += score;
		UpdateText();
	}

	private void UpdateText()
	{
		string scoreString = score < 10 ? "0" + score : "" + score;

		scoreText.text = scoreString;
	}
}
