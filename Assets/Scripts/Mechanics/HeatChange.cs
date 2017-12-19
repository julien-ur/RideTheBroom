using UnityEngine;

public class HeatChange : MonoBehaviour {

    public float heatPercent = 0;
    private ArduinoController arduino;

    void Start()
    {
        arduino = GameComponents.GetArduinoController();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            arduino.SetHeatPercent(heatPercent);
        }
    }

    void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            arduino.SetHeatToDefaultHeat();
        }
    }
}