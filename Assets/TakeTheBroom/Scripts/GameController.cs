using UnityEngine;

public class GameController : MonoBehaviour {

    public Score score;
    public HUD hud;

    public void ringActivated()
    {
        score.addRing();
        hud.show("" + score.getActivatedRings(), 3);
    }
}