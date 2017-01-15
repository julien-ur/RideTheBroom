using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public PlayerControl player;
    public Score score;
    public HUD hud;

    public void freezePlayer()
    {
        player.speed = 0;
    }

    internal void ringActivated()
    {
        score.addRing();
        hud.show("" + score.getActivatedRings());
    }
}