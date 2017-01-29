using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

    private Text gui;

    void Start()
    {
        gui = GetComponentInChildren<Text>();
        gui.enabled = false;
    }

    public void show(string text, float duration)
    {
        StartCoroutine(showText(text, duration));
    }

    IEnumerator showText(string text, float duration)
    {
        gui.enabled = true;
        gui.text = text;
        yield return new WaitForSeconds(duration);
        gui.enabled = false;
    }
}