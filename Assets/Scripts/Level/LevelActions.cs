using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelActions : MonoBehaviour {

    private Menu menu;
    private GameController gc;
    private PlayerControl pc;
    private Tutorial tut;
    private Scene scene;

    private Constants.LEVEL currentLevel;

    void Start () {
        menu = GameComponents.GetMenu();
        if (menu) deactivateTestingStuff(); // deactivates all objects of the layer "testing stuff"

        gc = GameComponents.GetGameController();
        pc = GameComponents.GetPlayerControl();
        tut = GameComponents.GetTutorial();

        scene = SceneManager.GetActiveScene();
        currentLevel = (Constants.LEVEL)(scene.buildIndex);

        StartCoroutine(LevelStartRoutine());
    }

    IEnumerator LevelStartRoutine()
    {
        yield return new WaitForSeconds(1);

        /*
         * TODO:
         * - Lande Besenkammer
         * - Öffne Tor
         * - Überprüfe Level
         * - Wenn Tutorial, trigger Tutorial Action
         * - Wenn nicht, show Countdown -> Starte Besen
         * 
         */

        if (menu) menu.hideMenu();

        if (currentLevel == Constants.LEVEL.Tutorial)
        {
            tut.TriggerAction(Constants.TUTORIAL_ACTION.Start);
        }
        else if (menu)
            gc.StartGameAfterCountdown();

        else
            gc.StartGame();

    }

    private void deactivateTestingStuff()
    {
        GameObject[] unwantedStuff = GameComponents.GetTestingStuff();
        foreach (GameObject o in unwantedStuff)
        {
            o.SetActive(false);
        }
    }
}
