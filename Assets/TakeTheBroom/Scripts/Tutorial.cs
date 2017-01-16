
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour {

    public HUD hud;
    public GameObject[] waypoints;

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

    void Update()
    {
        //if ((gc.getPlayerPos() - activeWaypoint.transform.position).magnitude < distance)
        //{

        //}
    }

    IEnumerator startTutorial(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);
        hud.show("Hallo ich bin der And! Ich nehme dich jetzt mit auf ein wildes Abenteuer!", 4);
        yield return new WaitForSeconds(4);
        hud.show("Lass uns vorher noch schnell deinen Besen checken!", 4);
        yield return new WaitForSeconds(4);
        hud.show("Lehne dich jetzt bitte mal nach rechts..", 4);
        yield return new WaitForSeconds(4);
        hud.show("Ok lass uns losfliegen! Huiiiiii!", 4);
        yield return new WaitForSeconds(4);
        gc.startBroom();
        //activeWaypoint = 
    }
}