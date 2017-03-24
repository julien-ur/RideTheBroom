using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelActions : MonoBehaviour
{
	private Transform player;
	private CompassControl compass;
    private Menu menu;
    private GameController gc;
    private PlayerControl pc;
    private Tutorial tut;
    private Scene scene;

    private Constants.LEVEL currentLevel;

    public GameObject ringContainer;


    void Start () {
        menu = GameComponents.GetMenu();
        if (menu) deactivateTestingStuff(); // deactivates all objects of the layer "testing stuff"

        gc = GameComponents.GetGameController();
        pc = GameComponents.GetPlayerControl();
        tut = GameComponents.GetTutorial();
        player = pc.GetComponent<Transform>();
        compass = player.GetComponentInChildren<CompassControl>();

        StartCoroutine(LevelStartRoutine());
    }

    IEnumerator LevelStartRoutine()
    {
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

        if (menu) menu.hideMenu();

        scene = SceneManager.GetActiveScene();
        currentLevel = (Constants.LEVEL)(scene.buildIndex);

        if (currentLevel == Constants.LEVEL.Tutorial)
        {
            tut.TriggerAction(Constants.TUTORIAL_ACTION.Start);
        }
        else if (menu)
            gc.StartGameAfterCountdown();

        else
            gc.StartGame();

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

    	if(nearestRing) compass.PointAtTarget(nearestRing);
    }
}
