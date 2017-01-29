
using System.Collections;
using UnityEngine;

public class Tutorial : MonoBehaviour {

    public HUD hud;
    public Wisp wisp;
    public Transform player;

    private GameController gc;
    private Fading fade;
    private object activeWaypoint;
    private float playerRotationDetectionAngle = 30;
    private bool triggered = false;

    void Start () {
        gc = GetComponent<GameController>();
        fade = GetComponent<Fading>();

        gc.freezeBroom();
        wisp.freeze(true);

        float fadingTimeInSec = fade.fadeIn();
        StartCoroutine(startTutorial(fadingTimeInSec));
	}

    public void trigger()
    {
        triggered = true;
    }

    IEnumerator startTutorial(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);

        hud.show("Hallo Fremder, ich bin der Wisp. Willkommen in meiner magischen Welt.", 3);
        yield return new WaitForSeconds(3);
        hud.show("Damit du gewappnet bist, für die Gefahren, die dich bald erwarten werden, werde ich dir zuerst ein paar grundlegende Dinge beibringen.", 3);
        yield return new WaitForSeconds(3);

        hud.show("Lehne dich jetzt bitte mal nach rechts..", 3);
        float lastAngle = player.eulerAngles.y;
        float angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.eulerAngles.y, ref angleCounter, true));

        hud.show("Schön!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach links..", 3);
        lastAngle = player.eulerAngles.y;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.eulerAngles.y, ref angleCounter, false));

        hud.show("Super!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach hinten..", 3);
        lastAngle = player.eulerAngles.x;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.eulerAngles.x, ref angleCounter, false));

        hud.show("Excellent!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach vorne..", 3);
        lastAngle = player.eulerAngles.x;
        angleCounter = 0;
        yield return new WaitUntil(() => hasPlayerExecutedClaimedRotation(ref lastAngle, player.eulerAngles.x, ref angleCounter, true));

        hud.show("Grandios!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Ok lass uns losfliegen! Huiiiiii!", 3);
        yield return new WaitForSeconds(3);

        wisp.freeze(false);
        gc.startBroom(5);

        yield return new WaitUntil(() => triggered);
        triggered = false;
        gc.slowDownBroom();
        hud.show("Siehst du die Ringe da vorne! Versuche durch alle hindurchzufliegen, du Lurch!", 4);
        yield return new WaitForSeconds(4);
        gc.startBroom(8);
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