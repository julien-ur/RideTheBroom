using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameController : MonoBehaviour
{
    private const string HIGHSCORE_FILE_PATH = "highscore.txt";
    private const string GHOSTMODE_LOG_PATH = "ghostmodetest.txt";

    struct GhostModePathNode
    {
        Vector3 position;
        float time;

        public GhostModePathNode(Vector3 position, float time)
        {
            this.position = position;
            this.time = time;
        }

        public string ToString()
        {
            return time.ToString("F3") + " " + position.x.ToString("F3") + " " + position.y.ToString("F3") + " " + position.z.ToString("F3") + "\r\n";
        }
    }

    private GameObject player;
    private Tutorial tutorial;
    private HUD hud;
    private Wisp wisp;
    private PlayerControl pc;
    private Fading fade;

    private float levelTime;
    private bool isGamePaused;

    private int numRings;

    private List<GhostModePathNode> ghostModePathLog;
    private float ghostModeLogTimer;
    private float lastGhostModeLogTime;
    private bool isGhostModeLogRunning;

    private void Start()
    {
        player = GameComponents.GetPlayer();
        hud = GameComponents.GetHUD();
        wisp = GameComponents.GetWisp();
        tutorial = GameComponents.GetTutorial();
        fade = GameComponents.GetFading();
        pc = GameComponents.GetPlayerControl();

        fade.fadeIn(1);
    }

    void Update()
    {
        if(!isGamePaused) levelTime += Time.deltaTime;
        HandleGhostModeLog();
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
        hud.show("Fancy Countdown missing", 3);
        yield return new WaitForSeconds(3);
        StartGame();
    }


    public void StartTutorial()
    {
        tutorial.StartTutorial();
    }

    public void LoadLevel(Constants.LEVEL lvl)
    {
        StartCoroutine(LoadScene(lvl));
    }

    public void StartGame()
    {
        levelTime = 0;
        numRings = 0;
        UnpauseGame();
        StartGhostModeLog();
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
        Debug.Log("Finished! Time: " + createTimeString(levelTime) + " Rings: " + numRings);
        SaveHighscoreFile();
        StopGhostModeLog();
        SaveGhostModeLog();
    }

    public void PauseGame()
    {
        isGamePaused = true;
    }

    public void UnpauseGame()
    {
        isGamePaused = false;
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

    private void HandleGhostModeLog()
    {
        if(isGhostModeLogRunning)
        {
            ghostModeLogTimer += Time.deltaTime;

            if(ghostModeLogTimer - lastGhostModeLogTime >= 1)
            {
                ghostModePathLog.Add(new GhostModePathNode(player.transform.position, ghostModeLogTimer));
                lastGhostModeLogTime = ghostModeLogTimer;
            }
        }
    }

    public void SaveGhostModeLog()
    {
        string text = "";

        foreach(GhostModePathNode node in ghostModePathLog)
        {
            text += node.ToString();
        }

        System.IO.File.WriteAllText(GHOSTMODE_LOG_PATH, text);
    }

    public void StartGhostModeLog()
    {
        ghostModePathLog = new List<GhostModePathNode>();
        isGhostModeLogRunning = true;
        ghostModeLogTimer = 0;
        lastGhostModeLogTime = 0;
    }

    public void StopGhostModeLog()
    {
        isGhostModeLogRunning = false;
    }
}