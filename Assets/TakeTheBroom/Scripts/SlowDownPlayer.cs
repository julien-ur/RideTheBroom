using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDownPlayer : MonoBehaviour {

    public enum TYPE { speedboost, slowdown }
    public enum METHOD { velocity, addForce, translate };

    public float thrust = 100;
    public float speed = 2;
    public METHOD usedMethod;
    public TYPE speedChangeType;

    void Start () {
		
	}
	
	void OnTriggerEnter (Collider col) {
        PlayerControl player;
		if (player = col.GetComponent<PlayerControl>())
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (speedChangeType == TYPE.slowdown)
            {
                rb.drag = 50;
                StartCoroutine(slowDownPlayer(player));
            }
            else
            {
                switch (usedMethod)
                {
                    case METHOD.velocity:
                        rb.velocity = transform.forward * thrust;
                        break;

                    case METHOD.addForce:
                        rb.AddForce(transform.forward * thrust, ForceMode.VelocityChange);
                        break;

                    case METHOD.translate:
                        player.momentum += transform.forward * thrust * 45 * Time.deltaTime;
                        break;
                }
            } 
        }
	}

    IEnumerator slowDownPlayer(PlayerControl player)
    {
        player.lockToTargetSpeed(1, 0.5f);
        yield return new WaitForSeconds(0.5f);
        player.unlockSpeed(2f);
    }
}
