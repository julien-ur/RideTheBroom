using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCabinTrigger : MonoBehaviour {

    public EventHandler PlayerLeftTheBuilding;

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            OnPlayerLEftTheBuilding();
        }
    }

    protected virtual void OnPlayerLEftTheBuilding()
    {
        if (PlayerLeftTheBuilding != null)
            PlayerLeftTheBuilding(this, EventArgs.Empty);
    }
}

