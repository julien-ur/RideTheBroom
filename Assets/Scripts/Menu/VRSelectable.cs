using System.Collections;
using UnityEngine;

public abstract class VRSelectable : MonoBehaviour {

    public abstract void OnPlayerFocusEnter();
    public abstract void OnPlayerFocusExit();
}
