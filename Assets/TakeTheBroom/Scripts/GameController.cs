using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public GameObject player;
    public HUD hud;
    public Wisp wisp;

    [HideInInspector]
    public PlayerControl pc;

    private Fading fade;
    private Score score;

    enum BuildIndex { Menu, Tutorial, FloatingRocks };
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
    
    public void LoadLevel(int id)
    {
        if ((BuildIndex)(id) == BuildIndex.Tutorial) StartCoroutine(learnBroomControl());
        else StartCoroutine(loadScene(id));
    }

    IEnumerator loadScene(int id)
    {
        if((BuildIndex)(id) > BuildIndex.Tutorial)
        {
            hud.show("Let's Go!", 3);
            yield return new WaitForSeconds(3);
        }
        
        BuildIndex sceneIndex = (BuildIndex)(SceneManager.GetActiveScene().buildIndex);
        if ((BuildIndex)(id) == sceneIndex) yield break;

        LoadSceneMode mode = (sceneIndex == BuildIndex.Menu) ? LoadSceneMode.Additive : LoadSceneMode.Single;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(id, mode);
        yield return new WaitUntil(() => loadOp.isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(id));

        if ((BuildIndex)(id) > BuildIndex.Tutorial)  pc.unlockSpeed(1);
        if ((BuildIndex)(id) == BuildIndex.Menu) yield break;

        Destroy(GameObject.Find("Menu Objects"));
        Destroy(GameObject.Find("Menu Canvas"));
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

        StartCoroutine(loadScene(1));
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