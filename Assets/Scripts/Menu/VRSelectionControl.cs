using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSelectionControl : MonoBehaviour {

    private bool objectSelected = false;


    public void OnVRSelection()
    {
        objectSelected = true;
    }

    public void ResetVRSelection()
    {
        objectSelected = false;
    }

    public bool IsObjectSelected()
    {
        return objectSelected;
    }
}