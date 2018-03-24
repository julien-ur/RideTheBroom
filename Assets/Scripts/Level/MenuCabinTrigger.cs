using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCabinTrigger : MonoBehaviour {

    public EventHandler PlayerLeftTheBuilding;

    void OnTriggerLeave(Collider other)
    {
        if (other.name == "Player")
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
