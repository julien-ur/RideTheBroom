using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

    private Text gui;

    void Awake()
    {
        gui = GetComponentInChildren<Text>();
        gui.enabled = false;
    }

    public void show(string text)
    {
        StartCoroutine(showText(text));
    }

    IEnumerator showText(string text)
    {
        gui.text = text;
        gui.enabled = true;
        yield return new WaitForSeconds(3);
        gui.enabled = false;
    }
}