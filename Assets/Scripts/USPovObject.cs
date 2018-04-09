using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USPovObject : MonoBehaviour
{
    public Light PresentationLight;

    public static EventHandler<ItemTriggerEventArgs> PovAdmirationComplete;
    public static EventHandler LastWish;

    private float _lightStartIntensity;

    void Awake()
    {
        _lightStartIntensity = PresentationLight.intensity;
    }

    public void Activate()
    {
        StopAllCoroutines();
        StartCoroutine(Activating());
    }

    public void Deactivate(Action callback)
    {
        StopAllCoroutines();
        StartCoroutine(Deactivating(callback));
    }

    public void LifeMakesNoSenseNow()
    {
        StopAllCoroutines();
        StartCoroutine(Finale());
    }

    public void StartSelectionProcess()
    {
        StopAllCoroutines();
        StartCoroutine(OnSelecting());
    }

    public void StopSelectionProcess()
    {
        StopAllCoroutines();
        StartCoroutine(RevertSelection());
    }

    private IEnumerator Activating()
    {
        PresentationLight.GetComponent<Light>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        yield return new WaitUntil(() =>
        {
            PresentationLight.range += 8;
            return PresentationLight.range >= 50;
        });
    }

    private IEnumerator Deactivating(Action callback)
    {
        yield return new WaitUntil(() =>
        {
            PresentationLight.range -= 8;
            return PresentationLight.range == 0;
        });
        PresentationLight.range = 0;
        PresentationLight.GetComponent<Light>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        callback();
    }

    private IEnumerator Finale()
    {
        GameObject povVisibilityContainer = transform.parent.parent.gameObject;

        var selectables = povVisibilityContainer.GetComponentsInChildren<USPovSelectable>();
        foreach (USPovSelectable s in selectables)
        {
            s.enabled = false;
        }

        float timer = 0;
        while ((timer += Time.deltaTime) < 0.08f)
        {
            transform.localScale += Vector3.one * 0.03f;
            PresentationLight.intensity += 3.5f;
            yield return new WaitForEndOfFrame();
        }

        Destroy(povVisibilityContainer.transform.parent.gameObject);
    }

    private IEnumerator OnSelecting()
    {
        float timer = 0;
        while((timer += Time.deltaTime) < 1.5f)
        {
            PresentationLight.range += 3;
            PresentationLight.intensity += 0.03f;
            yield return new WaitForEndOfFrame();
        }

        OnPovAdmirationComplete();
        LifeMakesNoSenseNow();
    }

    private IEnumerator RevertSelection()
    {
        while (PresentationLight.range > 50)
        {
            PresentationLight.range -= 10;
            yield return new WaitForEndOfFrame();
        }
        PresentationLight.intensity = _lightStartIntensity;
        PresentationLight.range = 50;
    }

    protected virtual void OnPovAdmirationComplete()
    {
        if (PovAdmirationComplete != null)
            PovAdmirationComplete(this, new ItemTriggerEventArgs() { Item = ItemTrigger.ITEM.Pov });
    }
}
