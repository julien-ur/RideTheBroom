using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	public bool invertVertical 	= false;
	public bool invertHorizontal 	= false;
	private int invertFactorVertical 	= -1;		// -1 = normal, 1 = inverted
	private int invertFactorHorizontal 	= -1;	// -1 = normal, 1 = inverted

	public float speedMin = 0.7f;
	public float speedMax = 15;
	public float speed = 3;	// units per second
	public float speedChange = 1;

	public float rotationFactorX = 9;
	public float rotationFactorY = 12;
	public float rotationFactorZ = 1;

	public float backRotationRate = 90;	// degrees per second to roll back camera

	private Transform transform;
	private Transform cameraTransform;

	void Awake()
	{
		transform 		= GetComponent<Transform>();
		cameraTransform = transform.GetChild(0).GetComponent<Transform>();
	}

	// Use this for initialization
	void Start ()
	{
		invertFactorVertical 	= (invertVertical 	? 1 : -1);
		invertFactorHorizontal 	= (invertHorizontal ? 1 : -1);
	}
	
	// Update is called once per frame
	void Update ()
	{
		// between 0 and 1
		float inputVertical 	= invertFactorVertical 		* Input.GetAxis("Vertical");
		float inputHorizontal 	= invertFactorHorizontal 	* Input.GetAxis("Horizontal");
		
		// TODO: use quaternions to avoid gimbal lock
		float rotateX = inputVertical 	* rotationFactorX;
		float rotateY = inputHorizontal * rotationFactorY * -1;
		float rotateZ = 0;

		float velocity;	// actual speed

		// Handle Speed Change
		if(Input.GetAxis("SlowDown") > 0 && speed > speedMin)
		{
			speed -= speedChange * Time.deltaTime;
			if(speed < speedMin) speed = speedMin;
		}
		else if(Input.GetAxis("Accelerate") > 0 && speed < speedMax)
		{
			speed += speedChange * Time.deltaTime;
			if(speed > speedMax) speed = speedMax;
		}

		velocity = speed * Time.deltaTime;

		// perform actual transformations
		transform.Rotate(rotateX, rotateY, rotateZ);
		transform.Translate(Vector3.forward * velocity);

		// make sure the head is straight up when it should be (same as in CameraRotation.cs)
		if(transform.eulerAngles.z != 0)	// roll back to zero degrees
		{
			float backRotationDegrees = backRotationRate * Time.deltaTime;

			// if close to zero, set to zero
			// transform.eulerAngles.z = 0 does not work because of quaternion magic
			if(Mathf.Abs(backRotationDegrees) > Mathf.Abs(transform.eulerAngles.z))
			{
				transform.Rotate(0, 0, -transform.eulerAngles.z);
			}
			else // roll back at fixed rate
			{
				// find direction to avoid accidental 360° rolls (and vomit)
				if(transform.eulerAngles.z < 180) backRotationDegrees *= -1;

				transform.Rotate(0, 0, backRotationDegrees);
			}
		}
	}
}
