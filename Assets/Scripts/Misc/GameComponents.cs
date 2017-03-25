using System.Collections.Generic;
using UnityEngine;

public static class GameComponents {

    public static GameController GetGameController()
    {
        GameObject gc = GetGameControl();
        return (gc) ? gc.GetComponent<GameController>() : null;
    }

    public static Tutorial GetTutorial()
    {
        GameObject gc = GetGameControl();
        return (gc) ? gc.GetComponent<Tutorial>() : null;
    }

    public static Fading GetFading()
    {
        GameObject gc = GetGameControl();
        return (gc) ? gc.GetComponent<Fading>() : null;
    }

    public static GhostModeController GetGhostModeController()
    {
        GameObject gc = GetGameControl();
        return (gc) ? gc.GetComponent<GhostModeController>() : null;
    }

    public static PlayerControl GetPlayerControl()
    {
        GameObject player = GetPlayer();
        return (player) ? player.GetComponent<PlayerControl>() : null;
    }

    public static Wisp GetWisp()
    {
        GameObject wo = GameObject.FindGameObjectWithTag("Wisp");
        return (wo) ? wo.GetComponent<Wisp>() : null;
    }

    public static HUD GetHUD()
    {
        GameObject ho = GameObject.FindGameObjectWithTag("HUD");
        return (ho) ? ho.GetComponent<HUD>() : null;
    }

    public static GameObject GetPlayer()
    {
        return GameObject.FindGameObjectWithTag("Player");
    }

    public static GameObject[] GetTestingStuff()
    {
        return FindGameObjectsWithLayer("TestingStuff");
    }

    public static GameObject GetMenuObject()
    {
        return GameObject.FindGameObjectWithTag("Menu");
    }

    public static Menu GetMenu()
    {
        GameObject mo = GameObject.FindGameObjectWithTag("Menu");
        return (mo) ? mo.GetComponent<Menu>() : null;
    }

    private static GameObject GetGameControl()
    {
        return GameObject.FindGameObjectWithTag("GameControl");
    }

    // Source: http://answers.unity3d.com/comments/362406/view.html (modified for use with string based layer name)
    private static GameObject[] FindGameObjectsWithLayer(string layerName)
    {
        GameObject[] goArray = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        List<GameObject> goList = new List<GameObject>();
        int layer = LayerMask.NameToLayer(layerName);

        for (int i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == layer)
            {
                goList.Add(goArray[i]);
            }
        }

        if (goList.Count == 0) { return null; }
        return goList.ToArray();
    }
}