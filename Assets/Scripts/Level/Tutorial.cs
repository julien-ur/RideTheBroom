
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour {

    private GameController gc;
    private GameObject player;
    private PlayerControl pc;
    private HUD hud;
    private Wisp wisp;

    private float playerRotationDetectionAngle = 30;
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
        //float duration = wisp.saySomething();
        //yield return new WaitForSeconds(duration + 1);

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
        hud.show("Ok lass uns losfliegen! Huiiiiii!", 3);
        wisp.initWaypoints();
        yield return new WaitForSeconds(3);
        gc.StartGame();
    }

    IEnumerator ExplainRings()
    {
        pc.changeSpeedToTargetSpeed(2, 0.5f);
        wisp.changeSpeedToTargetSpeed(3, 0.5f);
        hud.show("Siehst du die Ringe da vorne! Versuche durch alle hindurchzufliegen, du Lurch!", 4);
        yield return new WaitForSeconds(4);
        pc.changeSpeedToDefaultSpeed(0.5f);
        wisp.changeSpeedToDefaultSpeed(0.5f);
    }

    IEnumerator ExplainWindzone()
    {
        pc.changeSpeedToTargetSpeed(2, 1);
        wisp.changeSpeedToTargetSpeed(3, 1);
        hud.show("Das da vorne, das so merkwürdig flimmert, das ist eine Windzonen. Wenn du genau durch sie hindurchfliegst, beschleunigt dein Besen und du wirst richt schnell. Flieg einfach mir nach!", 4);
        yield return new WaitForSeconds(4);
        pc.changeSpeedToDefaultSpeed(0.5f);
        wisp.changeSpeedToDefaultSpeed(0.5f);
    }

    IEnumerator ExplainSpeedBoost()
    {
        pc.changeSpeedToTargetSpeed(2, 0.5f);
        wisp.changeSpeedToTargetSpeed(3, 0.5f);
        hud.show("Siehst du die bläuliche Energiekugel da vorne! Das ist eine Beschleuniger. Wenn du durch ihn Hindurchfliegst, wirst du regelrecht nach vorne geschleudert.", 4);
        yield return new WaitForSeconds(4);
        pc.changeSpeedToDefaultSpeed(0.5f);
        wisp.changeSpeedToDefaultSpeed(0.5f);
    }

    IEnumerator ExplainSlowDown()
    {
        float slowDownFactor = 0.1f;
        Time.timeScale = slowDownFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        hud.show("Huiiii ganz schön schnell. Die Wolke vor dir ist dazu da um zu bremsen. Besonders vor Kurven, oder nach einem Speedboost ist das hilfreich um auf der Strecke zu bleiben", 4 * slowDownFactor);
        yield return new WaitForSeconds(4 * slowDownFactor);

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
        onBroomControlLearned();
        //StartCoroutine(learnBroomControlRoutine());
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