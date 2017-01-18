using UnityEngine;

public class RingDetection : MonoBehaviour {

    public GameController gc;

    void OnTriggerExit(Collider col)
    {
        MagicalRing mr = col.GetComponent<MagicalRing>();
        if (mr && !mr.isActivated())
        {
            mr.setActivated();
            gc.ringActivated();
        }
    }
}