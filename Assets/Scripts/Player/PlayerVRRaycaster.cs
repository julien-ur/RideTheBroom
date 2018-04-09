using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VR;

public class PlayerVRRaycaster : MonoBehaviour {

    public LayerMask layerMask;
    private List<VRSelectable> _lastHits;

    void Awake()
    {
        _lastHits = new List<VRSelectable>();
    }

    void Update () {

        // Vector3 fwd = transform.TransformDirection(Vector3.forward);

        Debug.DrawRay(transform.position, transform.forward * 100);
        Ray r = new Ray(transform.position, transform.forward * 100);
        RaycastHit[] hits = Physics.RaycastAll(r, 100, layerMask, QueryTriggerInteraction.Collide);

        List<VRSelectable> currentHits = new List<VRSelectable>();

        foreach (RaycastHit hit in hits)
        {
            var hitSelectable = hit.transform.GetComponent<VRSelectable>();
            if (hitSelectable == null) return;

            currentHits.Add(hitSelectable);

            if (!_lastHits.Contains(hitSelectable))
            {
                hitSelectable.OnPlayerFocusEnter();
                _lastHits.Add(hitSelectable);
            }
        }

        if (_lastHits.Count == 0) return;

        var removedHits = _lastHits.Except(currentHits).ToList();
        foreach (VRSelectable removedHitSelectable in removedHits)
        {
            if (removedHitSelectable && removedHitSelectable.enabled)
                removedHitSelectable.OnPlayerFocusExit();

            _lastHits.Remove(removedHitSelectable);
        }
    }
}