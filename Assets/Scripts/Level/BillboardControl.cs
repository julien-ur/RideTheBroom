using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardControl : MonoBehaviour
{
    private TextMesh scoreMesh;
    private TextMesh highScoreMesh;

    void Start () {
        scoreMesh = transform.Find("Score").GetComponent<TextMesh>();
        highScoreMesh = transform.Find("High Score").GetComponent<TextMesh>();
        SetScore(new Score(5, 120f), new Score(100, 80f));
    }

    public void SetScore(Score score, Score highScore)
    {
        string scoreText = "Deine Runde\n" +
        "Magische Ringe: " + score.rings + "\n" + 
        "Flugzeit: " + (int)(score.timeInSec / 60) + "min " + score.timeInSec % 60 + "sec\n";

        string highScoreText = "\n\nBeste Hexe: " + highScore.rings + " Ringe / " + (int)(highScore.timeInSec / 60) + "min " + highScore.timeInSec % 60 + "sec";

        scoreMesh.text = scoreText;
        highScoreMesh.text = highScoreText;
    }
}