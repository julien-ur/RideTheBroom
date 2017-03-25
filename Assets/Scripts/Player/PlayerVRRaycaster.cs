using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVRRaycaster : MonoBehaviour {

    private VRSelectable currentHit;
    private VRSelectable lastHit;

    void Update () {

        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        Debug.DrawRay(transform.position, fwd * 50);
        if (Physics.Raycast(transform.position, fwd * 50, out hit))
        {
            currentHit = hit.transform.gameObject.GetComponent<VRSelectable>();
        }
        else
        {
            currentHit = null;
        }

        if (currentHit != lastHit)
        {
            if (lastHit) lastHit.OnPlayerFocusExit();
            if (currentHit) currentHit.OnPlayerFocusEnter();
            lastHit = currentHit;
        }
    }
}