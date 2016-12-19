using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalTrigger : MonoBehaviour {

    public GameObject boulders;
    private Rigidbody[] rbList;

    void Start () {
        rbList = boulders.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbList)
        {
            rb.isKinematic = true;
        }
    }

    void OnTriggerEnter () {
        foreach (Rigidbody rb in rbList)
        {
            rb.isKinematic = false;
        }
    }
}
