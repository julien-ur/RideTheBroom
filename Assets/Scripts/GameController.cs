using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    private GameObject player;
    private Tutorial tutorial;
    private HUD hud;
    private Wisp wisp;
    private PlayerControl pc;
    private Fading fade;

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
        pc.startBroom();
        if (wisp) wisp.startFlying();
    }

    public void StartGameAfterCountdown()
    {
        StartCoroutine(StartGameAfterCountdownRoutine());
    }

    public void RingActivated()
    {
        hud.show("Ring Activated", 2);
    }

    public void ShowResults(float durationInSec)
    {
        //hud.show("Ringe: " + score.getActivatedRings() + "  --  Zeit: " + Time.realtimeSinceStartup, durationInSec);
    }

}