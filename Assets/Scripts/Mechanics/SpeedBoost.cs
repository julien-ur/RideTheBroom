using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public float thrust = 25;

    void OnTriggerEnter(Collider col)
    {
        PlayerControl player;

        if (player = col.GetComponent<PlayerControl>())
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            rb.AddForce(player.transform.forward * thrust, ForceMode.VelocityChange);
        }
    }
}