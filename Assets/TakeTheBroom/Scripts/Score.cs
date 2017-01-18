using UnityEngine;

public class Score : MonoBehaviour {

    private int activatedRings;
    private int totalRings;

    public void addRing()
    {
        activatedRings++;
    }

    public int getActivatedRings()
    {
        return activatedRings;
    }
}