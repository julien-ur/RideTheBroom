using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFinish : MonoBehaviour
{
	private GameController gc;

	void Start ()
	{
		gc = GameComponents.GetGameController();
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.name == "Player")
		{
			gc.FinishLevel();
            GetComponent<Collider>().enabled = false;
			//Debug.Log("FINISH - " + createTimeString(levelTime));
		}
	}
}