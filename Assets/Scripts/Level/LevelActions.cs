using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelActions : MonoBehaviour
{
	private Transform player;
	private CompassControl compass;
    private Menu menu;
    private GameController gc;
    private PlayerControl pc;
    private Tutorial tut;
    private BroomCloset broomCloset;

    private Constants.LEVEL currentLevel;

    public GameObject ringContainer;


    void Start () {
        menu = GameComponents.GetMenu();
        if (menu) deactivateTestingStuff(); // deactivates all objects of the layer "testing stuff"

        gc = GameComponents.GetGameController();
        pc = GameComponents.GetPlayerControl();
        tut = GameComponents.GetTutorial();
        player = pc.GetComponent<Transform>();
        broomCloset = GameComponents.GetBroomCloset();
        compass = player.GetComponentInChildren<CompassControl>();

        StartCoroutine(LevelStartRoutine());
    }

    IEnumerator LevelStartRoutine()
    {
        pc.EnableRotation();
        yield return new WaitForSeconds(1);

        /*
         * TODO:
         * - Lande Besenkammer
         * - Öffne Tor
         * - Überprüfe Level
         * - Wenn Tutorial, trigger Tutorial Action
         * - Wenn nicht, show Countdown -> Starte Besen
         * 
         */
        currentLevel = gc.GetActiveLevel();

        if (menu)
        {
            Transform wispTrans = GameComponents.GetWisp().transform;
            player.parent = broomCloset.transform;
            wispTrans.parent = broomCloset.transform;
            float landingDuration = broomCloset.StartLanding();
            yield return new WaitForSeconds(landingDuration);
            player.parent = null;
            wispTrans.parent = null;

            float doorOpenDuration = broomCloset.OpenDoors();
            yield return new WaitForSeconds(doorOpenDuration);

            if (currentLevel == Constants.LEVEL.Tutorial)
            {
                tut.TriggerAction(Constants.TUTORIAL_ACTION.Start);
            }
            else
            {
                gc.StartGameAfterCountdown();
            }
        }
        else
        {
            gc.StartGame();
        }
    }

    private void deactivateTestingStuff()
    {
        GameObject[] unwantedStuff = GameComponents.GetTestingStuff();
        foreach (GameObject o in unwantedStuff)
        {
            o.SetActive(false);
        }
    }

    public Transform GetNearestRing(Transform playerPosition)
    {
    	if(ringContainer == null) return null;

    	MagicalRing[] rings = ringContainer.GetComponentsInChildren<MagicalRing>();

    	Transform nearest = null;
    	float nearestDistance = 0;

    	foreach(MagicalRing ring in rings)
    	{
    		if(nearest == null || (Vector3.Distance(ring.transform.position, playerPosition.position) < nearestDistance && ring.IsActive()))
    		{
    			nearest = ring.transform;
    			nearestDistance = Vector3.Distance(ring.transform.position, playerPosition.position);
    		}
    	}

    	return nearest;
    }

    void Update()
    {
    	Transform nearestRing = GetNearestRing(player);

    	if(nearestRing && compass) compass.PointAtTarget(nearestRing);
    }
}
