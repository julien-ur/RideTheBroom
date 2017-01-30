using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

    private Text gui;

    void Start()
    {
        gui = GetComponentInChildren<Text>();
    }

    public void show(string text, float duration)
    {
        StartCoroutine(showText(text, duration));
    }

    IEnumerator showText(string text, float duration)
    {
        gui.text = text;
        gui.enabled = true;
        yield return new WaitForSeconds(duration);
        gui.text = "";
    }
}