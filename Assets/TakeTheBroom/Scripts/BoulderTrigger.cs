using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderTrigger : MonoBehaviour {

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
        Time.timeScale = 0.2F;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;

        foreach (Rigidbody rb in rbList)
        {
            rb.isKinematic = false;
        }
    }
}