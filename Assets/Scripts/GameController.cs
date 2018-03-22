using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameController : MonoBehaviour
{
    private const string HIGHSCORE_FILE_PATH = "highscore.txt";

    private GameObject player;
    private Tutorial tutorial;
    private Wisp wisp;
    private PlayerControl pc;
    private ArduinoController arduino;
    private Fading fade;
    private GhostModeController ghostModeController;
    private MaterialResetter materialResetter;
    private VRSelectionControl vrSelectionControl;
    private List<Score> scores = new List<Score>();

    private float levelTime;
    private bool isGamePaused;

    private int numRings;

    void Start()
    {
        player = GameComponents.GetPlayer();
        wisp = GameComponents.GetWisp();
        tutorial = GameComponents.GetTutorial();
        fade = GameComponents.GetFading();
        pc = GameComponents.GetPlayerControl();
        ghostModeController = GameComponents.GetGhostModeController();
        materialResetter = GameComponents.GetMaterialResetter();
        vrSelectionControl = GameComponents.GetVRSelectionControl();

        pc.DisableRotation();
        StartCoroutine(GameStartRoutine());
    }

    IEnumerator GameStartRoutine()
    {
        fade.FadeIn(1);
        yield return new WaitForSeconds(3f);
        //float duration = wisp.talkToPlayer(wisp.MenuIntro);
        //yield return new WaitForSeconds(duration + 0.3f);
    }

    void Update()
    {
        if(!isGamePaused) levelTime += Time.deltaTime;
    }


    IEnumerator LoadScene(Constants.LEVEL levelToLoad)
    {
        if (levelToLoad == Constants.LEVEL.Tutorial)
        {
            float duration = wisp.talkToPlayer(wisp.TutorialFinished);
            yield return new WaitForSeconds(duration);
        }
        else if (levelToLoad == Constants.LEVEL.FloatingRocks)
        {
            float duration = wisp.talkToPlayer(wisp.FloatingRocksSelected);
            yield return new WaitForSeconds(duration);
        }
        else if(levelToLoad == Constants.LEVEL.ForestCave)
        {
            float duration = wisp.talkToPlayer(wisp.ForestCaveSelected);
            yield return new WaitForSeconds(duration);
        }
        
        Constants.LEVEL currentLevel = (Constants.LEVEL)(SceneManager.GetActiveScene().buildIndex);
        if (levelToLoad == currentLevel) yield break;

        LoadSceneMode mode = (currentLevel == Constants.LEVEL.Menu) ? LoadSceneMode.Additive : LoadSceneMode.Single;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync((int)(levelToLoad), mode);
        yield return new WaitUntil(() => loadOp.isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)(levelToLoad)));

        if (levelToLoad == Constants.LEVEL.Menu)
        {
            // wisp.talkToPlayer(wisp.BackToMenu);
            LoadLevel(Constants.LEVEL.FloatingRocks);
        }
    }

    IEnumerator StartGameAfterCountdownRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        StartGame();
    }


    public void StartTutorial()
    {
    	pc.EnableRotation();
        tutorial.StartTutorial();
    }

    public void LoadLevel(Constants.LEVEL lvl)
    {
    	pc.EnableRotation();

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
        pc.startBroom();
        ghostModeController.StartGhostModeLog(player.GetComponent<Transform>());
        if (wisp) wisp.startFlying();
    }

    public void StartGameAfterCountdown()
    {
        StartCoroutine(StartGameAfterCountdownRoutine());
    }

    public void RingActivated()
    {
        //player.Find("armature_score").GetComponent<ScoreDisplayControl>().AddScore(1);
        numRings++;
        player.GetComponentInChildren<ScoreDisplayControl>().AddScore(1);
    }

    public void ShowResults(float durationInSec)
    {
    }

    public void FinishLevel()
    {
        PauseGame();
        scores.Add(GetCurrentScore());
        Debug.Log("Finished! Time: " + createTimeString(levelTime) + " Rings: " + numRings);
        SaveHighscoreFile();
        ghostModeController.StopGhostModeLog();
        ghostModeController.SaveGhostModeLog();

        StartCoroutine(LoadMenu());
    }

    public Score GetCurrentScore()
    {
        return new Score(numRings, levelTime);
    }

    public Score GetHighScore()
    {
        float bestRingsPerSec = 0;
        Score bestScore = new Score(0, 0);

        foreach (Score s in scores)
        {
            float ringsPerSec = s.rings / s.timeInSec;
            if (ringsPerSec > bestRingsPerSec)
            {
                bestRingsPerSec = ringsPerSec;
                bestScore = s;
            }
        }
        return bestScore;
    }

    IEnumerator LoadMenu()
    {

        if (GetActiveLevel() == Constants.LEVEL.Tutorial)
        {
            float duration = wisp.talkToPlayer(wisp.FinishedMountainWorld);
            //yield return new WaitForSeconds(duration + 1f);
            yield return new WaitForSeconds(5f);
        }
        else
        {
            yield return new WaitForSeconds(5f);
        }
        fade.FadeOut(1);
        LoadLevel(Constants.LEVEL.Menu);
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