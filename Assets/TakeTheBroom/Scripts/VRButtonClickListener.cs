using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Utils;

public class VRButtonClickListener : MonoBehaviour {

    [SerializeField] private int btn_id = 0;
    [SerializeField] private GameObject Menu;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject LevelMenu;

    private GameController gc;


    void Start()
    {
        GetComponent<SelectionSlider>().OnBarFilled += VRButtonClicked;
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    private void VRButtonClicked()
    {
        if(btn_id == 0)
        {
            Menu.SetActive(false);
            gc.LoadLevel(1);
        }
        else if (btn_id == 1)
        {
            MainMenu.SetActive(false);
            LevelMenu.SetActive(true);
            gc.LoadLevel(2);
        }
    }
}
