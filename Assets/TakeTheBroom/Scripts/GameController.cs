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
        Transform[] ringList = route.GetComponentsInChildren<Transform>();
        totalRings = ringList.Length-2; // ignore parent and finish trigger
    }

    public void addPoint()
    {
        actualRings++;
        if(actualRings >= totalRings)
        {
            Debug.Log("Route finished!");
        }
        else
        {
            Debug.Log(actualRings + " from " + totalRings);
        }
    }

    public void finishedRoute()
    {
        Debug.Log("You only got " + actualRings + " out of " + totalRings);
    }
}