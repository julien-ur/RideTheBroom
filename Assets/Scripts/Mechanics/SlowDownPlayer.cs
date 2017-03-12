using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDownPlayer : MonoBehaviour {

    public enum TYPE { speedboost, slowdown }
    public enum METHOD { velocity, addForce, translate };

    public float thrust = 100;
    public float slowdownSpeed = 2;
    public METHOD usedMethod;
    public TYPE speedChangeType;

	
	void OnTriggerEnter (Collider col) {
        PlayerControl player;
		if (player = col.GetComponent<PlayerControl>())
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (speedChangeType == TYPE.slowdown)

                StartCoroutine(slowDownPlayer(player, rb));

            else
            {
                switch (usedMethod)
                {
                    case METHOD.velocity:
                        rb.velocity = player.transform.forward * thrust;
                        break;

                    case METHOD.addForce:
                        rb.AddForce(player.transform.forward * thrust, ForceMode.VelocityChange);
                        break;
                }
            } 
        }
	}

    IEnumerator slowDownPlayer(PlayerControl player, Rigidbody rb)
    {
        rb.drag = 50;
        player.changeSpeedToTargetSpeed(Constants.SLOWDOWN_TARGET_SPEED, Constants.SLOWDOWN_TIME);
        yield return new WaitForSeconds(Constants.SLOWDOWN_TIME);
        player.changeSpeedToDefaultSpeed(Constants.SLOWDOWN_RECOVERY_TIME);
        rb.drag = 1;
    }
}