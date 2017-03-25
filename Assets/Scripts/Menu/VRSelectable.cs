using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSelectable : MonoBehaviour {

    public Material tintMaterial;
    public Color focusedColor = new Color(0.6691177f, 0.8220081f, 1);
    public Color selectedColor = Color.white; //new Color(0.6691177f, 0.8220081f, 1);

    private const float tintEmission = 3;
    private const float tintTime = 0.5f;
    private const float tintCooldownTime = 0.2f;

    private const float blinkIntervall = 0.07f;
    private const float blinkHoldDuration = 0.14f;
    private const float blinkCount = 3;

    private Coroutine lastCoroutine;
    private bool selected = false;

    public void OnPlayerFocusEnter()
    {
        if (selected) return;

        if (lastCoroutine != null) StopCoroutine(lastCoroutine);
        lastCoroutine = StartCoroutine(startSelectionProcess());
    }

    public void OnPlayerFocusExit()
    {
        if (selected) return;

        if (lastCoroutine != null) StopCoroutine(lastCoroutine);
        lastCoroutine = StartCoroutine(unselect());
    }

    private void OnSelected()
    {
        selected = true;
        GetComponent<MenuAction>().OnVRSelection();
    }

    IEnumerator startSelectionProcess()
    {
        // start tinting painting
        float emission = 0;

        while (emission < tintEmission)
        {
            emission += Time.deltaTime / (tintTime / tintEmission);
            tintMaterial.SetColor("_EmissionColor", focusedColor * emission);
            yield return new WaitForEndOfFrame();
        }

        // blink before selection
        float blinkTime = 0;

        while (blinkTime < blinkCount * (blinkIntervall + blinkHoldDuration))
        {
            float blinkingCycleTime = blinkTime % (blinkIntervall + blinkHoldDuration);

            if (blinkingCycleTime > blinkIntervall)
            {
                emission = Mathf.SmoothStep(1, tintEmission, (blinkingCycleTime - blinkIntervall) / blinkHoldDuration);
            }
            else
            {
                emission = Mathf.Lerp(tintEmission, 1, blinkingCycleTime / blinkIntervall * 5); ;
            }

            tintMaterial.SetColor("_EmissionColor", focusedColor * emission);
            blinkTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        tintMaterial.SetColor("_EmissionColor", selectedColor);
        yield return new WaitForSeconds(0.5f);

        OnSelected();
    }

    IEnumerator unselect()
    {
        float emission = tintMaterial.GetColor("_EmissionColor").r / focusedColor.r;

        while (emission > 0)
        {
            emission -= Time.deltaTime / tintCooldownTime;
            emission = Mathf.Max(emission, 0);
            tintMaterial.SetColor("_EmissionColor", focusedColor * emission);
            yield return new WaitForEndOfFrame();
        }
    }
}
