using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelActions : MonoBehaviour
{
    public BillboardControl billboardControl;

    private Transform player;
	private CompassControl compass;
    private Menu menu;
    private GameObject[] menuProps;
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
        menuProps = GameComponents.GetAdditionalMenuProps();
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

        currentLevel = gc.GetActiveLevel();

        if (menu)
        {
            Wisp wisp = GameComponents.GetWisp();
            Transform wispTrans = wisp.transform;
            wisp.GetComponent<Animator>().enabled = false;

            player.parent = broomCloset.transform;
            wispTrans.parent = broomCloset.transform;
            float landingDuration = broomCloset.StartLanding();
            yield return new WaitForSeconds(landingDuration + 0.5f);

            player.parent = null;
            wispTrans.parent = null;
            foreach (GameObject p in menuProps)
            {
                p.SetActive(false);
            }

            if (currentLevel == Constants.LEVEL.Tutorial)
            {

                float duration = wisp.talkToPlayer(wisp.ArrivalMountainWorld);
                yield return new WaitForSeconds(duration);
            }
            else if (currentLevel == Constants.LEVEL.FloatingRocks)
            {
                float duration = wisp.talkToPlayer(wisp.ArrivalFloatingRocks);
                yield return new WaitForSeconds(duration);

            }

            float doorOpenDuration = broomCloset.OpenDoors();
            yield return new WaitForSeconds(doorOpenDuration);

            if (currentLevel == Constants.LEVEL.Tutorial)
            {
                // tut.TriggerAction(Constants.TUTORIAL_ACTION.Start);
                gc.StartGameAfterCountdown();
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

    public void FinishLevel()
    {
        pc.changeSpeedToTargetSpeed(0, 0.5f);
        foreach (GameObject p in menuProps)
        {
            p.SetActive(false);
        }
        gc.FinishLevel();

        billboardControl.SetScore(gc.GetCurrentScore(), gc.GetHighScore());
    }

    public Transform GetNearestRing(Transform playerPosition)
    {
    	if(ringContainer == null) return null;

    	MagicalRing[] rings = ringContainer.GetComponentsInChildren<MagicalRing>();

    	Transform nearest = null;
    	float nearestDistance = 0;

    	foreach(MagicalRing ring in rings)
    	{
    		if (ring.IsActive() && (nearest == null || Vector3.Distance(ring.transform.position, playerPosition.position) < nearestDistance) && Vector3.Distance(ring.transform.position, playerPosition.position) > 4)
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
