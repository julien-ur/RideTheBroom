using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public GameObject route;
    public Transform[] ringList;
    public MagicalRing nextRing;
    public Text pointDisplay;
    public int rings;
    public int ringCount = 0;

    void Awake () {
        ringList = route.GetComponentsInChildren<Transform>();
        rings = ringList.Length-1;
        nextRing = ringList[ringCount+1].GetComponent<MagicalRing>();
        //pointDisplay.text = ringCount + "/" + rings;
    }

    void Update ()
    {
        if(nextRing.activated)
        {
            next();
        }
    }
	
	public void next ()
    {
        ringCount++;

        if (ringCount >= rings)
        {
            //pointDisplay.text = "I'm done..";
        }
        else
        {
            nextRing = ringList[ringCount+1].GetComponent<MagicalRing>();
            //pointDisplay.text = ringCount + "/" + rings;
        }
    }
}