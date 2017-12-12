using UnityEngine;

public class HeatChange : MonoBehaviour {

    public int heat = 0;
    private ArduinoController arduino;

    void Start()
    {
        arduino = GameComponents.GetArduinoController();
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            arduino.SetHeat(heat);
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
