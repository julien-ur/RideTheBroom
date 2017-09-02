using System.Collections;
using UnityEngine;

public class BillboardControl : MonoBehaviour
{
    private TextMesh scoreMesh;
    private TextMesh highScoreMesh;

    private Renderer scoreRenderer;
    private Renderer highScoreRenderer;

    private float textFadingTimeInSec = 0.5f;

    void Start ()
    {
        GameObject scoreObj = transform.Find("Score").gameObject;
        GameObject highScoreObj = transform.Find("High Score").gameObject;

        scoreMesh = scoreObj.GetComponent<TextMesh>();
        highScoreMesh = highScoreObj.GetComponent<TextMesh>();

        scoreRenderer = scoreObj.GetComponent<Renderer>();
        highScoreRenderer = highScoreObj.GetComponent<Renderer>();

        ChangeAlphaOfText(0);
    }

    public void SetScore(Score score, Score highScore)
    {
        string scoreText = "Deine Runde\n" +
        "Magische Ringe: " + score.rings + "\n" + 
        "Flugzeit: " + Mathf.FloorToInt(score.timeInSec / 60) + "min " + (int)(score.timeInSec % 60) + "sec\n";

        string highScoreText = "\n\nBeste Hexe: " + highScore.rings + " Ringe / " + (int)(highScore.timeInSec / 60) + "min " + (int)(score.timeInSec % 60) + "sec";

        scoreMesh.text = scoreText;
        highScoreMesh.text = highScoreText;

        StartCoroutine(TextFadeIn());
    }

    IEnumerator TextFadeIn()
    {
        float alpha = 0;

        while (alpha < 1)
        {
            alpha += textFadingTimeInSec * Time.deltaTime;
            ChangeAlphaOfText(alpha);

            yield return new WaitForEndOfFrame();
        }
    }

    private void ChangeAlphaOfText(float a)
    {
        Color scoreColor = scoreRenderer.material.color;
        Color highScoreColor = highScoreRenderer.material.color;

        scoreColor.a = a;
        highScoreColor.a = a;

        scoreRenderer.material.color = scoreColor;
        highScoreRenderer.material.color = highScoreColor;
    }
}