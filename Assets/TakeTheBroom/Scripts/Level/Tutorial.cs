
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour {

    private GameController gc;
    private PlayerControl pc;
    private HUD hud;
    private Wisp wisp;

    private object activeWaypoint;
    private bool triggered = false;

    void Start () {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        pc = gc.pc;
        hud = gc.hud;
        wisp = gc.wisp;

        StartCoroutine(tutorial());
    }

    IEnumerator tutorial()
    {
        yield return new WaitForSeconds(3);

        hud.show("Ok lass uns losfliegen! Huiiiiii!", 3);
        wisp.initWaypoints();
        yield return new WaitForSeconds(3);

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
        gc.showResults(5);

        yield return new WaitForSeconds(5);
        gc.LoadLevel(0);
    }

    public void trigger()
    {
        triggered = true;
    }
}