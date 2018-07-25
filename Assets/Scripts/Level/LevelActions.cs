using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelActions : MonoBehaviour
{
    public GameObject RingContainer;
    public BillboardControl BillboardControl;
    public float DefaultHeatPercent = 0;

    private Transform _player;
	private CompassControl _compass;
    private Menu _menu;
    private GameObject[] _menuProps;
    private GameController _gc;
    private PlayerControl _pc;
    private BroomCloset _broomCloset;
    
    private Constants.LEVEL _currentLevel;

    private void Start () {
        _menu = GameComponents.GetMenu();
        if (_menu) deactivateTestingStuff(); // deactivates all objects of the layer "testing stuff"

        _gc = GameComponents.GetGameController();
        _pc = GameComponents.GetPlayerControl();
         _menuProps = GameComponents.GetAdditionalMenuProps();
        _player = _pc.GetComponent<Transform>();
        _broomCloset = GameComponents.GetBroomCloset();
        _compass = _player.GetComponentInChildren<CompassControl>();

        Text crosshair = GameComponents.GetVrHUD().transform.Find("Crosshair").GetComponentInChildren<Text>();
        crosshair.text = "";

        StartCoroutine(LevelStartRoutine());
    }

    private IEnumerator LevelStartRoutine()
    {
        _pc.EnableRotation();
        yield return new WaitForSeconds(1);

        _currentLevel = _gc.GetActiveLevel();

        if (_menu)
        {
            Wisp wisp = GameComponents.GetWisp();
            Transform wispTrans = wisp.transform;
            wisp.GetComponent<Animator>().enabled = false;

            _player.parent = _broomCloset.transform;
            wispTrans.parent = _broomCloset.transform;
            float landingDuration = _broomCloset.StartLanding();
            yield return new WaitForSeconds(landingDuration + 0.5f);

            _player.parent = null;
            wispTrans.parent = null;
            foreach (GameObject p in _menuProps)
            {
                p.SetActive(false);
            }

            if (_currentLevel == Constants.LEVEL.Tutorial)
            {
                float duration = wisp.talkToPlayer(wisp.ArrivalMountainWorld);
                yield return new WaitForSeconds(duration);
            }
            else if (_currentLevel == Constants.LEVEL.FloatingRocks)
            {
                float duration = wisp.talkToPlayer(wisp.ArrivalFloatingRocks);
                yield return new WaitForSeconds(duration);
            }
            else if (_currentLevel == Constants.LEVEL.ForestCave)
            {
                _pc.ChangeSpeed(22);
            }
            else if (_currentLevel == Constants.LEVEL.SpaceProcedural)
            {
                _pc.ChangeSpeed(20, 20, 20);
                _pc.gameObject.GetComponent<CapsuleCollider>().isTrigger = false;
                _pc.enableBroomRollback = false;
                _pc.tiltAcceleration = false;
                _pc.spaceTiltAcceleration = false;
            }

            float doorOpenDuration = _broomCloset.OpenDoors();
            yield return new WaitForSeconds(doorOpenDuration);

            if (_currentLevel == Constants.LEVEL.Tutorial)
            {
                // tut.TriggerAction(Constants.TUTORIAL_ACTION.Start);
                _gc.StartGameAfterCountdown();
            }
            else
            {
                _gc.StartGameAfterCountdown();
            }
        }
        else
        {
            _gc.StartGame();
        }

        HeatControl hc = _gc.GetComponent<HeatControl>();
        if (hc) hc.SetDefaultHeatPercent(DefaultHeatPercent);
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
        _pc.ChangeSpeedToTargetSpeed(0, 0.5f);
        foreach (GameObject p in _menuProps)
        {
            p.SetActive(false);
        }
        _gc.FinishLevel();

        BillboardControl.SetScore(_gc.GetCurrentScore(), _gc.GetHighScore());
    }

    public Transform GetNearestRing(Transform playerPosition)
    {
    	if(RingContainer == null) return null;

    	MagicalRing[] rings = RingContainer.GetComponentsInChildren<MagicalRing>();

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
    	Transform nearestRing = GetNearestRing(_player);

    	if(nearestRing && _compass) _compass.PointAtTarget(nearestRing);
    }
}
