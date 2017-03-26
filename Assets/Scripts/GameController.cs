﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameController : MonoBehaviour
{
    private const string HIGHSCORE_FILE_PATH = "highscore.txt";

    private GameObject player;
    private Tutorial tutorial;
    private HUD hud;
    private Wisp wisp;
    private PlayerControl pc;
    private Fading fade;
    private GhostModeController ghostModeController;
    private MaterialResetter materialResetter;
    private VRSelectionControl vrSelectionControl;

    private float levelTime;
    private bool isGamePaused;

    private int numRings;

    void Start()
    {
        player = GameComponents.GetPlayer();
        hud = GameComponents.GetHUD();
        wisp = GameComponents.GetWisp();
        tutorial = GameComponents.GetTutorial();
        fade = GameComponents.GetFading();
        pc = GameComponents.GetPlayerControl();
        ghostModeController = GameComponents.GetGhostModeController();
        materialResetter = GameComponents.GetMaterialResetter();
        vrSelectionControl = GameComponents.GetVRSelectionControl();

        fade.fadeIn(1);
    }

    void Update()
    {
        if(!isGamePaused) levelTime += Time.deltaTime;
    }


    IEnumerator LoadScene(Constants.LEVEL levelToLoad)
    {
        hud.show("Let's Go!", 3);
        yield return new WaitForSeconds(3);

        Constants.LEVEL currentLevel = (Constants.LEVEL)(SceneManager.GetActiveScene().buildIndex);
        if (levelToLoad == currentLevel) yield break;

        LoadSceneMode mode = (currentLevel == Constants.LEVEL.Menu) ? LoadSceneMode.Additive : LoadSceneMode.Single;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync((int)(levelToLoad), mode);
        yield return new WaitUntil(() => loadOp.isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)(levelToLoad)));
    }

    IEnumerator StartGameAfterCountdownRoutine()
    {
        // implement countdown sound and visuals
        hud.show("Wisp Countdown", 3);
        yield return new WaitForSeconds(3);
        StartGame();
    }


    public void StartTutorial()
    {
        tutorial.StartTutorial();
    }

    public void LoadLevel(Constants.LEVEL lvl)
    {
        if(lvl == Constants.LEVEL.Menu)
        {
            vrSelectionControl.ResetVRSelection();
            materialResetter.ResetTintedMaterials();
        }

        StartCoroutine(LoadScene(lvl));
    }

    public void StartGame()
    {
        levelTime = 0;
        numRings = 0;
        UnpauseGame();
        ghostModeController.StartGhostModeLog(player.GetComponent<Transform>());
        pc.startBroom();
        if (wisp) wisp.startFlying();
    }

    public void StartGameAfterCountdown()
    {
        StartCoroutine(StartGameAfterCountdownRoutine());
    }

    public void RingActivated()
    {
        //hud.show("Ring Activated", 2);
        //player.Find("armature_score").GetComponent<ScoreDisplayControl>().AddScore(1);
        numRings++;
        player.GetComponentInChildren<ScoreDisplayControl>().AddScore(1);
    }

    public void ShowResults(float durationInSec)
    {
        //hud.show("Ringe: " + score.getActivatedRings() + "  --  Zeit: " + Time.realtimeSinceStartup, durationInSec);
    }

    public void FinishLevel()
    {
        PauseGame();
        pc.changeSpeedToTargetSpeed(0, 0.5f);
        Debug.Log("Finished! Time: " + createTimeString(levelTime) + " Rings: " + numRings);
        SaveHighscoreFile();
        ghostModeController.StopGhostModeLog();
        ghostModeController.SaveGhostModeLog();
    }

    public void PauseGame()
    {
        isGamePaused = true;
    }

    public void UnpauseGame()
    {
        isGamePaused = false;
    }

    public Constants.LEVEL GetActiveLevel()
    {
        Scene s = SceneManager.GetActiveScene();
        return (Constants.LEVEL)(s.buildIndex);
    }

    // TODO: move to somewhere else
    private string createTimeString(float time)
    {
        int seconds = (int) (time % 60);
        int minutes = (int) ((time / 60) % 60);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void SaveHighscoreFile()
    {
        string line = createTimeString(levelTime) + " " + numRings + "\r\n";
        
        System.IO.File.AppendAllText(HIGHSCORE_FILE_PATH, line);
    }

    void OnApplicationQuit()
    {
        if (materialResetter) materialResetter.ResetTintedMaterials();
    }
}