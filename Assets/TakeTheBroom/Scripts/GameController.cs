using UnityEngine;

public class GameController : MonoBehaviour {

    public PlayerControl player;
    public Score score;
    public HUD hud;

    public void freezeBroom()
    {
        player.speed = 0;
    }

    public void startBroom(float speed)
    {
        player.speed = speed;
    }

    public void slowDownBroom()
    {
        player.speed = 3;
    }

    public void ringActivated()
    {
        score.addRing();
        hud.show("" + score.getActivatedRings(), 3);
    }
}