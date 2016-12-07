using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public GameObject route;
    public Text pointDisplay;
    public int totalRings;
    public int actualRings = 0;

    void Awake () {
        //Transform[] ringList = route.GetComponentsInChildren<Transform>();
        //foreach (Transform ring in ringList)
        //{
        //    if (ring.name.Contains("MagicalRing")) totalRings++;
        //}

        pointDisplay.text = actualRings + "/" + totalRings;
    }

    public void addPoint()
    {
        actualRings++;
        if(actualRings >= totalRings)
        {
            Debug.Log("Route finished!");
            pointDisplay.text = "Route finished. All Rings collected!";
        }
        else
        {
            Debug.Log(actualRings + " from " + totalRings);
            pointDisplay.text = actualRings + "/" + totalRings;
        }
    }

    public void finishedRoute()
    {
        Debug.Log("You only got " + actualRings + " out of " + totalRings);
        pointDisplay.text = "Route finished with " + actualRings + "/" + totalRings;
    }
}