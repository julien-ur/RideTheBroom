using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindZoneScript : MonoBehaviour
{
	public float strength = 0.1f;
	public Vector3 direction = Vector3.forward;

	private Transform transform;

	void Awake()
	{
		transform = GetComponent<Transform>();
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnTriggerStay(Collider col)
	{
		PlayerControl player;
		if(player = col.GetComponent<PlayerControl>())
		{
			Debug.Log("Wind");
			player.momentum += transform.TransformDirection(direction) * strength * Time.deltaTime;
			//Debug.Log(direction + " - " + transform.TransformDirection(direction));
			//col.attachedRigidbody.AddForce(direction * strength * Time.deltaTime);

		}
	}
}
