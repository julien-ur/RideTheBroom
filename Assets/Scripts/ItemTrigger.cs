using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemTriggerEventArgs : EventArgs
{
    public ItemTrigger.ITEM Item { get; set; }
}

public class ItemTrigger : MonoBehaviour {

    public enum ITEM { RefillTank, Pov };
    public ITEM Item;
    public static EventHandler<ItemTriggerEventArgs> PlayerTriggered;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            OnPlayerTriggered();
            Destroy(gameObject, 2);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            //Destroy(gameObject);
        }
    }

    protected virtual void OnPlayerTriggered()
    {
        if (PlayerTriggered != null)
            PlayerTriggered(this, new ItemTriggerEventArgs { Item = this.Item });
    }
}
