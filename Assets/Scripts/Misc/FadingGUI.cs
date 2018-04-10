using UnityEngine;

public class FadingGui : MonoBehaviour
{

    public Texture2D fadeOutTexture;

    private float fadingTimeInSec = 3;
    private int drawDepth = -1000; // the texture's order in the draw hierachy: a low number means it renders on top
    private float alpha;
    private int fadeDir; // in = -1 or out = 1

    void OnGUI()
    {
        alpha += (Time.deltaTime / fadingTimeInSec) * fadeDir;
        alpha = Mathf.Clamp01(alpha);

        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        GUI.depth = drawDepth;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);
    }

    public void FadeIn(float f)
    {
        fadingTimeInSec = f;
        fadeDir = -1;
        alpha = 1;
    }

    public void FadeOut(float f)
    {
        fadingTimeInSec = f;
        fadeDir = 1;
        alpha = 0;
    }
}