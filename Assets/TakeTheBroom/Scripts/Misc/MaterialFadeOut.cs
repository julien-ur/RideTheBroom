using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialFadeOut : MonoBehaviour {

    private Transform mainCam;
    private Renderer r;
    private float matAlpha;

    void Start() {
        mainCam = Camera.main.transform;
        r = GetComponent<Renderer>();
        matAlpha = r.material.color.a;
    }

	void Update () {
        var cameraDist = (mainCam.position + 2*Vector3.forward - transform.position).magnitude;
        
        if(cameraDist <= 3)
        {
            Color c = r.material.color;
            float alpha = Mathf.Lerp(0, matAlpha, cameraDist/3);
            c.a = alpha;
            r.material.color = c;
        }

    }
}
