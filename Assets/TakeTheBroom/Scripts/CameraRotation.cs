using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {

	private Transform transform;

	public const float rotationFactorX = 1;
	public const float rotationFactorY = 1;
	public const float rotationFactorZ = 60;

	public const float backRotationRate = 60;	// degrees per second to roll back camera

	public const float balanceBoardFactorX = 1.0f;

	private PlayerControl player;

	// Use this for initialization
	void Start ()
	{
		transform = GetComponent<Transform>();
		player = transform.parent.GetComponent<PlayerControl>();	// PlayerControl script of player GameObject
	}
	
	// Update is called once per frame
	void Update ()
	{
		float inputHorizontal = 0;

		try
		{
			if(player.enableBalanceBoardControl && player.balanceBoard != null)
			{
				// use the balance board if it's enabled AND available
				inputHorizontal = player.balanceBoard.x * balanceBoardFactorX;
			}
			else
			{
				// else use normal input
				inputHorizontal = -1 * Input.GetAxis("Horizontal");
			}
		

			float rotateX = 0;
			float rotateY = 0;
			float rotateZ = inputHorizontal * rotationFactorZ * Time.deltaTime;

			
			// if joystick is pressed, roll camera view
			if(inputHorizontal != 0)
			{
				// cap rotation at 90° in each direction
				// TODO: cap as variable?
				if(transform.eulerAngles.z < 45 || transform.eulerAngles.z > 315)
				{
					transform.Rotate(rotateX, rotateY, rotateZ);
				}
			}
			else if(transform.localEulerAngles.z != 0)	// roll back to zero degrees
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
		catch(System.Exception e)
		{
			player.enableBalanceBoardControl = false;
		}
	}
}