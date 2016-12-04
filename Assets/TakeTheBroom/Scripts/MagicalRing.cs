using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicalRing : MonoBehaviour {

    public GameController gc;
    public bool finishTrigger = false;
    private bool activated = false;

    void OnTriggerExit(Collider col)
    {
		if(col.name == "WitchBroom_01" && !activated && !finishTrigger)
		{
        	activated = true;
            gc.addPoint();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.name == "WitchBroom_01" && finishTrigger)
        {
            activated = true;
            gc.finishedRoute();
        }
    }
}