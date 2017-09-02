
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour {

    private GameController gc;
    private GameObject player;
    private PlayerControl pc;
    private HUD hud;
    private Wisp wisp;

    private float playerRotationDetectionAngle = 50;
    private object activeWaypoint;
    private bool triggered = false;


    void Start () {
        gc = GameComponents.GetGameController();
        player = GameComponents.GetPlayer();
        hud = GameComponents.GetHUD();
        wisp = GameComponents.GetWisp();
        pc = GameComponents.GetPlayerControl();
    }

    IEnumerator learnBroomControlRoutine()
    {
        float duration = wisp.talkToPlayer(wisp.TutorialSelected);
        yield return new WaitForSeconds(duration);

        wisp.talkToPlayer(wisp.TurnBroomRight);
        float lastAngle = player.transform.eulerAngles.y;
        float angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.y, ref angleCounter, true));

        duration = wisp.talkToPlayer(wisp.ComplimentGrandios);
        yield return new WaitForSeconds(duration + 0.5f);

        wisp.talkToPlayer(wisp.TurnBroomLeft);
        lastAngle = player.transform.eulerAngles.y;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.y, ref angleCounter, false));

        duration = wisp.talkToPlayer(wisp.ComplimentZauberhaft);
        yield return new WaitForSeconds(duration + 0.5f);

        wisp.talkToPlayer(wisp.TurnBroomUp);
        lastAngle = player.transform.eulerAngles.x;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.x, ref angleCounter, false));

        duration = wisp.talkToPlayer(wisp.ComplimentUnglaublich);
        yield return new WaitForSeconds(duration + 0.5f);

        wisp.talkToPlayer(wisp.TurnBroomDown);
        lastAngle = player.transform.eulerAngles.x;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.x, ref angleCounter, true));

        duration = wisp.talkToPlayer(wisp.ComplimentMotiviert);
        yield return new WaitForSeconds(duration + 0.5f);

        onBroomControlLearned();
    }

    private bool hasPlayerExecutedClaimedRotation(ref float lastAngle, float currentAngle, ref float angleCounter, bool positiveDirection)
    {
        float deltaAngle = Mathf.DeltaAngle(lastAngle, currentAngle);

        if (deltaAngle >= 0 && positiveDirection || deltaAngle <= 0 && !positiveDirection)
        {
            angleCounter += Mathf.Abs(deltaAngle);
        }
        else angleCounter = 0;

        //Debug.Log("last: " + lastAngle + " current: " + currentAngle + " counter: " + angleCounter);
        lastAngle = currentAngle;

        if (angleCounter > playerRotationDetectionAngle) return true;
        else return false;
    }

    private void onBroomControlLearned()
    {
        gc.LoadLevel(Constants.LEVEL.Tutorial);
    }

    IEnumerator CallPlayerToFollow()
    {
        wisp.initWaypoints();
        yield return new WaitForSeconds(1);
        gc.StartGame();
    }

    IEnumerator ExplainRings()
    {
        pc.changeSpeedToTargetSpeed(2, 0.5f);
        wisp.changeSpeedToTargetSpeed(3f, 0.5f);
        float duration = wisp.talkToPlayer(wisp.ExplainRings);
        yield return new WaitForSeconds(duration);
        pc.changeSpeedToDefaultSpeed(0.5f);
        wisp.changeSpeedToDefaultSpeed(0.5f);
    }

    IEnumerator ExplainWindzone()
    {
        pc.changeSpeedToTargetSpeed(2, 1);
        wisp.changeSpeedToTargetSpeed(3f, 1);
        float duration = wisp.talkToPlayer(wisp.ExplainWindzones);
        yield return new WaitForSeconds(duration);
        pc.changeSpeedToDefaultSpeed(0.5f);
        wisp.changeSpeedToDefaultSpeed(0.5f);
    }

    IEnumerator ExplainSpeedBoost()
    {
        pc.changeSpeedToTargetSpeed(2, 0.5f);
        wisp.changeSpeedToTargetSpeed(3f, 0.5f);
        float duration = wisp.talkToPlayer(wisp.ExplainEnergyBoost);
        yield return new WaitForSeconds(duration);
        pc.changeSpeedToDefaultSpeed(0.5f);
        wisp.changeSpeedToDefaultSpeed(0.5f);
    }

    IEnumerator ExplainSlowDown()
    {
        float slowDownFactor = 0.1f;
        Time.timeScale = slowDownFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        float duration = wisp.talkToPlayer(wisp.ExplainSlowCloud);
        yield return new WaitForSeconds(duration * slowDownFactor);

        while (Time.timeScale < 1)
        {
            Time.timeScale += 2 * Time.deltaTime;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator ExplainHardRoute()
    {
        float slowDownFactor = 0.1f;
        Time.timeScale = slowDownFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        hud.show("Aaah moment, warte. Siehst du das Schild da vorne?! Das steht überall da wo es alternative Streckenabschnitte gibt. Doch sei gewarnt, diese Absschnitte sind zumeist besonders schwer zu meistern und nicht geeignet für unerfahrerne Abenteurer!", 6 * slowDownFactor);
        yield return new WaitForSeconds(6 * slowDownFactor);

        while (Time.timeScale < 1)
        {
            Time.timeScale += 2 * Time.deltaTime;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return new WaitForEndOfFrame();
        }
    }

    public void StartTutorial()
    {
        //onBroomControlLearned();
        StartCoroutine(learnBroomControlRoutine());
    }

    public void TriggerAction(Constants.TUTORIAL_ACTION action)
    {
        switch (action) {

            case Constants.TUTORIAL_ACTION.Start:
                StartCoroutine(CallPlayerToFollow());
                break;

            case Constants.TUTORIAL_ACTION.Rings:
                StartCoroutine(ExplainRings());
                break;

            case Constants.TUTORIAL_ACTION.WindZone:
                StartCoroutine(ExplainWindzone());
                break;

            case Constants.TUTORIAL_ACTION.SpeedBoost:
                StartCoroutine(ExplainSpeedBoost());
                break;

            case Constants.TUTORIAL_ACTION.SlowDown:
                StartCoroutine(ExplainSlowDown());
                break;

            case Constants.TUTORIAL_ACTION.HardRoute:
                StartCoroutine(ExplainHardRoute());
                break;
        }
    }
}