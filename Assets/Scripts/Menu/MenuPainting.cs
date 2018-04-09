using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPainting : VRSelectable
{
    public Material TintMaterial;
    public Color FocusedColor = new Color(0.6691177f, 0.8220081f, 1);
    public Color SelectedColor = Color.white; //new Color(0.6691177f, 0.8220081f, 1);

    private const float TintEmission = 3;
    private const float TintTime = 0.5f;
    private const float TintCooldownTime = 0.2f;

    private const float BlinkIntervall = 0.07f;
    private const float BlinkHoldDuration = 0.14f;
    private const float BlinkCount = 3;

    private Coroutine _lastCoroutine;
    private MaterialResetter _materialResetter;
    private VRSelectionControl _selectionControl;
    private bool _selected = false;

    void Start()
    {
        _materialResetter = GameComponents.GetMaterialResetter();
    }

    public override void OnPlayerFocusEnter()
    {
        if (_selectionControl == null) _selectionControl = GameComponents.GetVRSelectionControl();

        if (_selected || _selectionControl.IsObjectSelected()) return;

        if (_lastCoroutine != null) StopCoroutine(_lastCoroutine);
        _lastCoroutine = StartCoroutine(Select());
    }

    public override void OnPlayerFocusExit()
    {
        if (_selected || _selectionControl.IsObjectSelected()) return;

        if (_lastCoroutine != null) StopCoroutine(_lastCoroutine);
        _lastCoroutine = StartCoroutine(UnSelect());
    }

    private IEnumerator OnSelected()
    {
        _selected = true;
        _selectionControl.OnVRSelection();
        _materialResetter.OnMaterialTinted(TintMaterial);
        yield return new WaitForSeconds(2f);
        GetComponent<MenuAction>().OnVRSelection();
    }

    private IEnumerator Select()
    {
        // start tinting painting
        float emission = 0;

        while (emission < TintEmission)
        {
            emission += Time.deltaTime / (TintTime / TintEmission);
            TintMaterial.SetColor("_EmissionColor", FocusedColor * emission);
            yield return new WaitForEndOfFrame();
        }

        // blink before selection
        float blinkTime = 0;

        while (blinkTime < BlinkCount * (BlinkIntervall + BlinkHoldDuration))
        {
            float blinkingCycleTime = blinkTime % (BlinkIntervall + BlinkHoldDuration);

            if (blinkingCycleTime > BlinkIntervall)
            {
                emission = Mathf.SmoothStep(1, TintEmission, (blinkingCycleTime - BlinkIntervall) / BlinkHoldDuration);
            }
            else
            {
                emission = Mathf.Lerp(TintEmission, 1, blinkingCycleTime / BlinkIntervall * 5); ;
            }

            TintMaterial.SetColor("_EmissionColor", FocusedColor * emission);
            blinkTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        TintMaterial.SetColor("_EmissionColor", SelectedColor);

        StartCoroutine(OnSelected());
    }

    private IEnumerator UnSelect()
    {
        float emission = TintMaterial.GetColor("_EmissionColor").r / FocusedColor.r;

        while (emission > 0)
        {
            emission -= Time.deltaTime / TintCooldownTime;
            emission = Mathf.Max(emission, 0);
            TintMaterial.SetColor("_EmissionColor", FocusedColor * emission);
            yield return new WaitForEndOfFrame();
        }
    }
}
