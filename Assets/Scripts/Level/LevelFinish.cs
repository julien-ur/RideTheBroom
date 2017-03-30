using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFinish : MonoBehaviour
{
	private LevelActions levelActions;

    void Start ()
	{
        levelActions = GameComponents.GetLevelActions();
    }

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag == "Player")
		{
            levelActions.FinishLevel();
            GetComponent<Collider>().enabled = false;
			//Debug.Log("FINISH - " + createTimeString(levelTime));
		}
	}
}