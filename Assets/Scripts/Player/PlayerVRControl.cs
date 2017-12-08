using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVRControl : MonoBehaviour {

	void Update () {
        transform.parent.rotation = transform.rotation;
    }
}
