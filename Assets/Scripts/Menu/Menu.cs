using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {

    public AnimationClip clip;

    private GameObject menu;
    private GameController gc;
    private bool actionExcuted = false;

    void Start()
    {
        gc = GameComponents.GetGameController();
        menu = GameComponents.GetMenuObject();
    }

	void Update () {
        //if (Input.GetKeyDown(KeyCode.A) && !actionExcuted)
        //{
        //    gc.StartTutorial();
        //    actionExcuted = true;
        //}
            
        //if (Input.GetKeyDown(KeyCode.B) && !actionExcuted)
        //{
        //    gc.LoadLevel(Constants.LEVEL.FloatingRocks);
        //    actionExcuted = true;
        //}
        if (Input.GetKeyDown(KeyCode.C) && !actionExcuted)
        {
            Debug.Log("Load Menu");
            gc.LoadLevel(Constants.LEVEL.Menu);
            actionExcuted = true;
        }
    }

    public void hideMenu()
    {
        menu.SetActive(false);
    }
}