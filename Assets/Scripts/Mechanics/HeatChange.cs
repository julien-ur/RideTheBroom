using UnityEngine;

public class HeatChange : MonoBehaviour {

    public float heatPercent = 0;
    private HeatControl hc;

    void Start()
    {
        hc = GameComponents.GetGameController().GetComponent<HeatControl>();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            hc.AddHeatSource(gameObject.GetInstanceID(), heatPercent);
        }
    }

    void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            hc.RemoveHeatSource(gameObject.GetInstanceID());
        }
    }
}