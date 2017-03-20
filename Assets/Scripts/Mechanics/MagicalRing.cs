using UnityEngine;

public class MagicalRing : MonoBehaviour {

    void OnTriggerExit(Collider col)
    {
        if (col.GetComponent<PlayerControl>())
        {
            GameComponents.GetGameController().RingActivated();
            GetComponent<MeshCollider>().enabled = false;
        }
    }
}