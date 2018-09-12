using UnityEngine;

public class ScoreDisplayControl : MonoBehaviour
{
	private TextMesh scoreText;
	private float score;

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

	public void SetScore(float score)
	{
		this.score = score;
		UpdateText();
	}

	public void AddScore(float score)
	{
		this.score += score;
	    if (score < 0) this.score = 0;
		UpdateText();
	}

	public float GetScore()
	{
		return score;
	}

	private void UpdateText()
	{

		string scoreString = score < 10 ? "0" + (int)score : "" + (int)score;

		scoreText.text = scoreString;
	}
}
