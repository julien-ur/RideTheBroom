using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicalRing : MonoBehaviour {

    public bool activated = false;

    void OnTriggerEnter(Collider col)
    {
    	//Debug.Log("Enter");
    	PlayerControl player;
		if(player = col.GetComponent<PlayerControl>())
		{
			//Debug.Log("Player");
        	activated = true;
        }
    }
}