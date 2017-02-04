
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour {

    public HUD hud;
    public Wisp wisp;
    public GameObject player;

    private Fading fade;
    private PlayerControl pc;
    private GameController gc;

    private object activeWaypoint;
    private float fadingTimeInSec;
    private float playerRotationDetectionAngle = 30;
    private bool triggered = false;

    void Start () {
        fade = GetComponent<Fading>();
        pc = player.GetComponent<PlayerControl>();
        gc = GetComponent<GameController>();

        pc.lockToTargetSpeed(0, 0);
        wisp.lockToTargetSpeed(0, 0);

        fadingTimeInSec = fade.fadeIn();
	}

    public void startTutorial()
    {
        Debug.Log("clicked");
        StartCoroutine(tutorial(fadingTimeInSec));
    }

    IEnumerator tutorial(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);

        //hud.show("Hallo Fremder, ich bin der Wisp. Willkommen in meiner magischen Welt.", 3);
        //yield return new WaitForSeconds(3);
        //hud.show("Damit du gewappnet bist, für die Gefahren, die dich bald erwarten werden, werde ich dir zuerst ein paar grundlegende Dinge beibringen.", 3);
        //yield return new WaitForSeconds(3);

        //hud.show("Lehne dich jetzt bitte mal nach rechts..", 3);
        //float lastAngle = player.transform.eulerAngles.y;
        //float angleCounter = 0;
        //yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.y, ref angleCounter, true));

        //hud.show("Schön!", 2);
        //yield return new WaitForSeconds(2);

        //hud.show("Lehne dich jetzt bitte mal nach links..", 3);
        //lastAngle = player.transform.eulerAngles.y;
        //angleCounter = 0;
        //yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.y, ref angleCounter, false));

        //hud.show("Super!", 2);
        //yield return new WaitForSeconds(2);

        //hud.show("Lehne dich jetzt bitte mal nach hinten..", 3);
        //lastAngle = player.transform.eulerAngles.x;
        //angleCounter = 0;
        //yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.x, ref angleCounter, false));

        //hud.show("Excellent!", 2);
        //yield return new WaitForSeconds(2);

        //hud.show("Lehne dich jetzt bitte mal nach vorne..", 3);
        //lastAngle = player.transform.eulerAngles.x;
        //angleCounter = 0;
        //yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.transform.eulerAngles.x, ref angleCounter, true));

        //hud.show("Grandios!", 2);
        //yield return new WaitForSeconds(2);

        //hud.show("Ok lass uns losfliegen! Huiiiiii!", 3);
        //yield return new WaitForSeconds(3);

        wisp.unlockSpeed(1);
        pc.unlockSpeed(1);

        yield return new WaitUntil(() => triggered);
        pc.lockToTargetSpeed(2, 0.5f);
        wisp.lockToTargetSpeed(3, 0.5f);
        hud.show("Siehst du die Ringe da vorne! Versuche durch alle hindurchzufliegen, du Lurch!", 4);
        yield return new WaitForSeconds(4);
        pc.unlockSpeed(0.5f);
        wisp.unlockSpeed(0.5f);
        triggered = false;

        yield return new WaitUntil(() => triggered);
        pc.lockToTargetSpeed(2, 1);
        wisp.lockToTargetSpeed(3, 1);
        hud.show("Das da vorne, das so merkwürdig flimmert, das sind Windzonen. Wenn du genau durch sie durchfliegst, beschleunigt dein Besen und du wirst richt schnell. Flieg einfach mir nach!", 4);
        yield return new WaitForSeconds(4);
        pc.unlockSpeed(0.5f);
        wisp.unlockSpeed(0.5f);
        triggered = false;

        yield return new WaitUntil(() => triggered);
        pc.lockToTargetSpeed(0, 0);
        wisp.lockToTargetSpeed(0, 0);
        gc.showResults();

        yield return new WaitForSeconds(4);
        float fadingTimeInSec = fade.fadeOut(3);
        yield return new WaitForSeconds(fadingTimeInSec);
        SceneManager.LoadScene(1);
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

    public void trigger()
    {
        triggered = true;
    }
}