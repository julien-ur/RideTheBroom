
using System.Collections;
using UnityEngine;

public class Tutorial : MonoBehaviour {

    public HUD hud;
    public Wisp wisp;
    public Transform player;

    private GameController gc;
    private Fading fade;
    private object activeWaypoint;
    private float playerMotionDetectionAngle = 20;
    private float xStartAngle;
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
        float startAngle = player.eulerAngles.y;
        yield return new WaitUntil(() => hasPlayerTurnedRight(startAngle));

        hud.show("Schön!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach links..", 3);
        startAngle = player.eulerAngles.y;
        yield return new WaitUntil(() => hasPlayerTurnedLeft(startAngle));

        hud.show("Super!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach hinten..", 3);
        startAngle = player.eulerAngles.x;
        yield return new WaitUntil(() => hasPlayerTurnedUp(startAngle));

        hud.show("Excellent!", 2);
        yield return new WaitForSeconds(2);

        hud.show("Lehne dich jetzt bitte mal nach vorne..", 3);
        startAngle = player.eulerAngles.x;
        yield return new WaitUntil(() => hasPlayerTurnedDown(startAngle));

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

    private bool hasPlayerTurnedRight(float yStartAngle)
    {
        float targetAngle = (yStartAngle + playerMotionDetectionAngle + 360) % 360;
        float deltaAngle = Mathf.DeltaAngle(player.eulerAngles.y, targetAngle);

        Debug.Log(player.eulerAngles.y + " " + targetAngle + " " + Mathf.Abs(deltaAngle));

        if (Mathf.Abs(deltaAngle) < 2) return true;
        return false;
    }

    private bool hasPlayerTurnedLeft(float yStartAngle)
    {
        float targetAngle = (yStartAngle - playerMotionDetectionAngle + 360) % 360;
        float deltaAngle = Mathf.DeltaAngle(player.eulerAngles.y, targetAngle);

        Debug.Log(player.eulerAngles.y + " " + targetAngle + " " + Mathf.Abs(deltaAngle));

        if (Mathf.Abs(deltaAngle) < 2) return true;
        return false;
    }

    private bool hasPlayerTurnedUp(float xStartAngle)
    {
        float targetAngle = (xStartAngle - playerMotionDetectionAngle + 360) % 360;
        float deltaAngle = Mathf.DeltaAngle(player.eulerAngles.x, targetAngle);

        Debug.Log(player.eulerAngles.x + " " + targetAngle + " " + Mathf.Abs(deltaAngle));

        if (Mathf.Abs(deltaAngle) < 2) return true;
        return false;
    }

    private bool hasPlayerTurnedDown(float xStartAngle)
    {
        float targetAngle = (xStartAngle + playerMotionDetectionAngle + 360) % 360;
        float deltaAngle = Mathf.DeltaAngle(player.eulerAngles.x, targetAngle);

        Debug.Log(player.eulerAngles.x + " " + targetAngle + " " + Mathf.Abs(deltaAngle));

        if (Mathf.Abs(deltaAngle) < 2) return true;
        return false;
    }
}