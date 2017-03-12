using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDown : MonoBehaviour {
	
	void OnTriggerEnter (Collider col) {

        PlayerControl player;

		if (player = col.GetComponent<PlayerControl>())
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            StartCoroutine(slowDownPlayer(player, rb));
        }
	}

    IEnumerator slowDownPlayer(PlayerControl player, Rigidbody rb)
    {
        float defaultDrag = rb.drag;
        rb.drag = 50;
        player.changeSpeedToTargetSpeed(Constants.SLOWDOWN_TARGET_SPEED, Constants.SLOWDOWN_TIME);
        yield return new WaitForSeconds(Constants.SLOWDOWN_TIME);
        player.changeSpeedToDefaultSpeed(Constants.SLOWDOWN_RECOVERY_TIME);
        rb.drag = defaultDrag;
    }
}