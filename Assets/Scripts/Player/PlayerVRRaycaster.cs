using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVRRaycaster : MonoBehaviour {

    private GameObject currentHit;
    private GameObject lastHit;
    private Material lastHitMaterial;
    private Coroutine lastCoroutine;

    private Color selectionColor = new Color(0.6691177f, 0.8220081f, 1);
    private Color chosenColor = Color.white; //new Color(0.6691177f, 0.8220081f, 1);

    private const float tintEmission = 3;
    private const float tintTime = 0.5f;
    private const float tintCooldownTime = 0.2f;

    void Update () {

        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        Debug.DrawRay(transform.position, fwd * 50);
        if (Physics.Raycast(transform.position, fwd * 50, out hit))
        {
            currentHit = hit.transform.gameObject;
        }
        else
            currentHit = null;


        if (currentHit != lastHit)
        {
            lastHit = currentHit;
            if (lastCoroutine != null) StopCoroutine(lastCoroutine);
            if (lastHitMaterial) lastCoroutine = StartCoroutine(unselect(lastHitMaterial));

            if (!currentHit) return;

            MeshRenderer r = currentHit.GetComponent<MeshRenderer>();

            foreach (Material mat in r.materials)
            {
                if (mat.name.Contains("picture"))
                {
                    lastHitMaterial = mat;
                    lastCoroutine = StartCoroutine(startSelectionProcess(mat));
                }
            }
        }

    }

    private void selectPainting()
    {
        currentHit.GetComponent<MenuAction>().OnVRSelection();
    }

    IEnumerator startSelectionProcess(Material mat)
    {
        float emission = 0;

        while (emission < tintEmission)
        {
            emission += Time.deltaTime / (tintTime / tintEmission);
            mat.SetColor("_EmissionColor", selectionColor * emission);
            yield return new WaitForEndOfFrame();
        }

        float blinkingTime = 0;
        float blinkIntervall = 0.07f;
        float blinkHoldDuration = 0.14f;
        float blinkCount = 3;

        while (blinkingTime < blinkCount * (blinkIntervall + blinkHoldDuration))
        {
            float blinkingCycleTime = blinkingTime % (blinkIntervall + blinkHoldDuration);

            if(blinkingCycleTime > blinkIntervall)
            {
                emission = Mathf.SmoothStep(1, tintEmission, (blinkingCycleTime - blinkIntervall) / blinkHoldDuration);
            }
            else
            {
                emission = Mathf.Lerp(tintEmission, 1, blinkingCycleTime / blinkIntervall * 5); ;
            }

            mat.SetColor("_EmissionColor", selectionColor * emission);
            blinkingTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        mat.SetColor("_EmissionColor", chosenColor);
        selectPainting();
    }

    IEnumerator unselect(Material mat)
    {
        float emission = mat.GetColor("_EmissionColor").r / selectionColor.r;

        while (emission > 0)
        {
            emission -= Time.deltaTime / tintCooldownTime;
            emission = Mathf.Max(emission, 0);
            mat.SetColor("_EmissionColor", selectionColor * emission);
            yield return new WaitForEndOfFrame();
        }
    }
}
