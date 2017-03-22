using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassControl : MonoBehaviour
{
	Transform compassArrow;
	//Transform dummyTarget;
	Transform player;
	float ang = 0;

	// Use this for initialization
	void Start ()
	{
		compassArrow = GetComponent<Transform>().Find("compass_arrow");
		//dummyTarget = GameObject.Find("floating_rock_dummy_01 (1)").transform;
		player = GetComponent<Transform>().parent.transform;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//ang += 45 * Time.deltaTime;
		//SetAngle(ang);
		//PointAtTarget(dummyTarget);
	}

	public void SetAngle(float angle) // in deg
	{
		compassArrow.Rotate(Vector3.forward, angle - compassArrow.localEulerAngles.z, Space.Self);
	}

	public void PointAtTarget(Transform target)
	{
		Vector3 delta = target.position - compassArrow.position;
		
		float angle = Quaternion.LookRotation(delta).eulerAngles.y;

		SetAngle(angle - player.eulerAngles.y);
	}
}
