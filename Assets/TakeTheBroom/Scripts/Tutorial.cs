
using System.Collections;
using UnityEngine;

public class Tutorial : MonoBehaviour {

    public HUD hud;
    public Wisp wisp;

    private GameController gc;
    private Fading fade;
    private object activeWaypoint;

    void Start () {
        gc = GetComponent<GameController>();
        fade = GetComponent<Fading>();

        gc.freezeBroom();
        float fadingTimeInSec = fade.fadeIn();


        StartCoroutine(startTutorial(fadingTimeInSec));
	}

    internal void trigger(int id)
    {
        StartCoroutine(startWaypointRoutine(id));
    }

    IEnumerator startTutorial(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);
        hud.show("Hallo ich bin der And! Ich nehme dich jetzt mit auf ein wildes Abenteuer!", 3);
        gc.freezeBroom();
        yield return new WaitForSeconds(3);
        hud.show("Lass uns vorher noch schnell deinen Besen checken!", 3);
        gc.freezeBroom();
        yield return new WaitForSeconds(3);
        hud.show("Lehne dich jetzt bitte mal nach rechts..", 3);
        gc.freezeBroom();
        yield return new WaitForSeconds(3);
        hud.show("Ok lass uns losfliegen! Huiiiiii!", 3);
        gc.freezeBroom();
        yield return new WaitForSeconds(3);
        gc.startBroom(5);
    }

    IEnumerator startWaypointRoutine(int id)
    {
        switch (id)
        {
            case 0:
                gc.slowDownBroom();
                hud.show("Siehst du die Ringe da vorne! Versuche durch alle hindurchzufliegen, du Lurch!", 4);
                yield return new WaitForSeconds(4);
                gc.startBroom(8);
                break;
        }
    }
}