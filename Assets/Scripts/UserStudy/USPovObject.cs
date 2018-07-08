using System;
using System.Collections;
using UnityEngine;

public class USPovObject : MonoBehaviour
{
    public Light PresentationLight;
    private float _lightStartIntensity;
    private const double SelectionTime = 0.8f;

    void Awake()
    {
        _lightStartIntensity = PresentationLight.intensity;
    }

    public void Activate()
    {
        StopAllCoroutines();
        StartCoroutine(Activating());
    }

    public void Deactivate()
    {
        StopAllCoroutines();
        StartCoroutine(Deactivating());
    }

    public void StartSelectionProcess(Action callback)
    {
        StopAllCoroutines();
        StartCoroutine(OnSelecting(callback));
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

    private IEnumerator Deactivating()
    {
        yield return new WaitUntil(() =>
        {
            PresentationLight.range -= 8;
            return PresentationLight.range == 0;
        });
        PresentationLight.range = 0;
        PresentationLight.GetComponent<Light>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }

    private IEnumerator OnSelecting(Action callback)
    {
        float timer = 0;
        while((timer += Time.deltaTime) < SelectionTime)
        {
            PresentationLight.range += 2;
            PresentationLight.intensity += 0.02f;
            yield return new WaitForEndOfFrame();
        }

        callback();
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

    private void LifeMakesNoSenseNow()
    {
        StopAllCoroutines();
        StartCoroutine(Finale());
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
            PresentationLight.intensity += 2f;
            yield return new WaitForEndOfFrame();
        }

        Destroy(povVisibilityContainer.transform.parent.gameObject);
    }
}
