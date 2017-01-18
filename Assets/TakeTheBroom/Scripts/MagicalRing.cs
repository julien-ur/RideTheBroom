using UnityEngine;

public class MagicalRing : MonoBehaviour {

    private bool activated = false;

    public void setActivated()
    {
        activated = true;
    }

    public bool isActivated()
    {
        return activated;
    }
}