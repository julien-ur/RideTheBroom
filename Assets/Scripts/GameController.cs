using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public enum LEVEL { Menu, Tutorial, FloatingRocks };

    public GameObject Menu;
    public GameObject MainMenu;
    public GameObject LevelMenu;
    public GameObject player;
    public HUD hud;
    public Wisp wisp;

    [HideInInspector]
    public PlayerControl pc;

    private Fading fade;
    private Score score;
    private float playerRotationDetectionAngle = 30;

    private void Start()
    {
        fade = GetComponent<Fading>();
        pc = player.GetComponent<PlayerControl>();
        score = player.GetComponent<Score>();

        pc.lockToTargetSpeed(0, 0);
        wisp.lockToTargetSpeed(0, 0);

        fade.fadeIn(1);
    }

    public void ringActivated()
    {
        score.addRing();
        hud.show("" + score.getActivatedRings(), 3);
    }

    public void showResults(float durationInSec)
    {
        hud.show("Ringe: " + score.getActivatedRings() + "  --  Zeit: " + Time.realtimeSinceStartup, durationInSec);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) LoadLevel((LEVEL.Tutorial));
        if (Input.GetKeyDown(KeyCode.B)) LoadLevel((LEVEL.FloatingRocks));
    }
    
    public void LoadLevel(LEVEL lvl)
    {
        
        if (lvl == LEVEL.Tutorial) StartCoroutine(learnBroomControl());
        else StartCoroutine(loadScene(lvl));
    }

    IEnumerator loadScene(LEVEL levelToLoad)
    {
        if(levelToLoad > LEVEL.Tutorial)
        {
            hud.show("Let's Go!", 3);
            yield return new WaitForSeconds(3);
        }
        
        LEVEL currentLevel = (LEVEL)(SceneManager.GetActiveScene().buildIndex);
        if (levelToLoad == currentLevel) yield break;

        LoadSceneMode mode = (currentLevel == LEVEL.Menu) ? LoadSceneMode.Additive : LoadSceneMode.Single;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync((int)(levelToLoad), mode);
        yield return new WaitUntil(() => loadOp.isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)(levelToLoad)));

        if (levelToLoad > LEVEL.Tutorial) pc.unlockSpeed(1);
        // if (levelToLoad != LEVEL.Menu) Destroy(GameObject.Find("Menu Props"));
    }

    IEnumerator learnBroomControl()
    {
        float duration = wisp.saySomething();
        yield return new WaitForSeconds(duration + 1);

        hud.show("Lehne dich jetzt bitte mal nach rechts..", 3);
        float lastAngle = player.transform.eulerAngles.y;
        float angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.y, ref angleCounter, true));

        hud.show("Schön!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach links..", 3);
        lastAngle = player.transform.eulerAngles.y;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.y, ref angleCounter, false));

        hud.show("Super!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach hinten..", 3);
        lastAngle = player.transform.eulerAngles.x;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.x, ref angleCounter, false));

        hud.show("Excellent!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach vorne..", 3);
        lastAngle = player.transform.eulerAngles.x;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.x, ref angleCounter, true));

        hud.show("Grandios!", 2);
        yield return new WaitForSeconds(2);

        StartCoroutine(loadScene(LEVEL.Tutorial));
    }

    private bool hasPlayerExecutedClaimedRotation(ref float lastAngle, float currentAngle, ref float angleCounter, bool positiveDirection)
    {
        float deltaAngle = Mathf.DeltaAngle(lastAngle, currentAngle);

        if (deltaAngle >= 0 && positiveDirection || deltaAngle <= 0 && !positiveDirection)
        {
            angleCounter += Mathf.Abs(deltaAngle);
        }
        else angleCounter = 0;

        // Debug.Log("last: " + lastAngle + " current: " + currentAngle + " counter: " + angleCounter);
        lastAngle = currentAngle;

        if (angleCounter > playerRotationDetectionAngle) return true;
        else return false;
    }
}