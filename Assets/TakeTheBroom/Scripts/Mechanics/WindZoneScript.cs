using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindZoneScript : MonoBehaviour
{
	void OnTriggerStay(Collider col)
	{
		PlayerControl player;

		if(player = col.GetComponent<PlayerControl>())
		{
            player.momentum += transform.up * Constants.WINDZONE_STRENGTH * Time.deltaTime;
		}
	}
}