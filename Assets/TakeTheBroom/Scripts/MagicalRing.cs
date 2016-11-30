using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicalRing : MonoBehaviour {

    public bool activated = false;

    void OnTriggerStay(Collider other)
    {
        activated = true;
    }
}