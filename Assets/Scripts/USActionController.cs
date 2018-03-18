using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USActionController : MonoBehaviour {

    public void OnActionStarted(object sender, USActionEventArgs args)
    {
        Debug.Log("Action Started " + args.ActionType + " " + args.Count);
    }

    public void OnActionSuccess(object sender, USActionEventArgs args)
    {
        Debug.Log("Action Success " + args.ActionType + " " + args.Count);
    }

    public void OnActionTimeOut(object sender, USActionEventArgs args)
    {
        Debug.Log("Action Timeout " + args.ActionType + " " + args.Count);
    }
}
