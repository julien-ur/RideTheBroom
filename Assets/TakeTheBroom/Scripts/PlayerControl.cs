using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	public const bool invertVertical 	= false;
	public const bool invertHorizontal 	= false;
	private int invertFactorVertical 	= -1;		// -1 = normal, 1 = inverted
	private int invertFactorHorizontal 	= -1;	// -1 = normal, 1 = inverted

	public float speed = 1;	// units per second

	public const float rotationFactorX = 9;
	public const float rotationFactorY = 12;
	public const float rotationFactorZ = 1;

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

		float velocity = speed * Time.deltaTime;	// actual speed

		transform.Rotate(rotateX, rotateY, rotateZ);
		//transform.RotateAround(transform.position, transform.right, rotateX);
		//transform.RotateAround(transform.position, transform.up, rotateY);

		transform.Translate(Vector3.forward * velocity);

		//if(!(cameraTransform.eulerAngles.z == 0 || cameraTransform.eulerAngles.z == 180)) Debug.Log(transform.eulerAngles.x);
	}
}
