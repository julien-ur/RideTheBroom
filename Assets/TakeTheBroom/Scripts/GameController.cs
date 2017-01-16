using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public PlayerControl player;
    public Score score;
    public HUD hud;

    public void freezeBroom()
    {
        player.speed = 0;
    }

    public void startBroom()
    {
        player.speed = 5;
    }

    public void ringActivated()
    {
        score.addRing();
        hud.show("" + score.getActivatedRings(), 3);
    }

}