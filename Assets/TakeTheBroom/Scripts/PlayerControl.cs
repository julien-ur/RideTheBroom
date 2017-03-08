using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VR;

public class PlayerControl : MonoBehaviour
{
	public bool invertVertical 	= false;
	public bool invertHorizontal 	= false;
	private int invertFactorVertical 	= -1;		// -1 = normal, 1 = inverted
	private int invertFactorHorizontal 	= -1;	// -1 = normal, 1 = inverted

	public float speedMin = 0.7f;
	public float speedMax = 15;
    public float defaultSpeed = 10; // units per second
    public float speedChange = 1;

	public float rotationFactorX = 60;
	public float rotationFactorY = 60;
	public float rotationFactorZ = 1;

	public float backRotationRate = 90;	// degrees per second to roll back camera

	public BalanceBoardInput balanceBoard;
	public bool enableBalanceBoardControl = false;	// use the balance board?
	
	// multiplier for each balance board axis to adjust rotation speed
	public float balanceBoardFactorX = 2.5f;
	public float balanceBoardFactorY = 2.0f;

	public bool enableCameraRollback = true;

	[SerializeField] VRNode m_VRNode    = VRNode.Head;

	[HideInInspector]
	public Vector3 momentum;

	private Transform transform;
	private Rigidbody rigidbody;
	private Transform cameraTransform;

    private float speed;
    private bool speedLocked = false;

    void Awake()
	{
		transform 		= GetComponent<Transform>();
		rigidbody 		= GetComponent<Rigidbody>();
		cameraTransform = transform.GetChild(0).GetComponent<Transform>();
		balanceBoard = GetComponent<BalanceBoardInput>();

        speed = defaultSpeed;
    }

    // Use this for initialization
    void Start ()
	{
		//momentum = Vector3.up; // temp
		invertFactorVertical 	= (invertVertical 	? 1 : -1);
		invertFactorHorizontal 	= (invertHorizontal ? 1 : -1);
	}
	
	// Update is called once per frame
	void Update ()
	{
		try
		{
			float inputVertical = 0;
			float inputHorizontal = 0;

			// +++ VR +++
			Quaternion q = InputTracking.GetLocalRotation(m_VRNode);
			//Quaternion q = Quaternion.identity;

            //Debug.Log(q.eulerAngles.ToString("F2"));

            //Debug.Log(balanceBoard.accY);

			if(enableBalanceBoardControl && balanceBoard != null)
			{
                // use the balance board if it's enabled AND available
                //inputHorizontal = balanceBoard.x * balanceBoardFactorX;
                //inputVertical 	= balanceBoard.y * balanceBoardFactorY;
                inputVertical = Mathf.Clamp((balanceBoard.accY - 500) / 50, -1.0f, 1.0f);

                if(q != null)
				{
	                //inputHorizontal = Mathf.Clamp(q.eulerAngles.x, -1.0f, 1.0f);
	                //inputHorizontal = Mathf.Clamp(Mathf.DeltaAngle(0, q.eulerAngles.z) / 180, -1.0f, 1.0f);
	                float normedRot = Mathf.DeltaAngle(0, q.eulerAngles.z) / 180;
	                inputHorizontal = Mathf.Clamp(Math.Sign(normedRot) * Mathf.Pow(normedRot, 2) * 30, -1.0f, 1.0f);
	            }
			}
			else
			{
				// instead use a gamepad
				inputVertical 		= invertFactorVertical 		* Input.GetAxis("Vertical");
				inputHorizontal 	= invertFactorHorizontal 	* Input.GetAxis("Horizontal");
			}

			
			
			
			// TODO: use quaternions to avoid gimbal lock
			float rotateX = inputVertical 	* rotationFactorX * Time.deltaTime;
			float rotateY = inputHorizontal * rotationFactorY * Time.deltaTime * -1;
			float rotateZ = 0;

			float velocity; // actual speed

            // Handle Speed Change
            //if (!speedLocked)
            //{
            //    if (Input.GetAxis("SlowDown") > 0 && speed > speedMin)
            //    {
            //        speed -= speedChange * Time.deltaTime;
            //        if (speed < speedMin) speed = speedMin;
            //    }
            //    else if (Input.GetAxis("Accelerate") > 0 && speed < speedMax)
            //    {
            //        speed += speedChange * Time.deltaTime;
            //        if (speed > speedMax) speed = speedMax;
            //    }
            //}
                
            velocity = speed * Time.deltaTime;

			// perform actual transformations
			//transform.Rotate(rotateX, rotateY, rotateZ); //deprecated
			// use axis-angle rotation because euler angles suck
			transform.RotateAround(transform.position, transform.right, rotateX);
			
			if(enableCameraRollback)
			{
				transform.RotateAround(transform.position, Vector3.up, rotateY);
			}
			else
			{
				transform.RotateAround(transform.position, transform.up, rotateY);
			}

			//transform.Translate((Vector3.forward + transform.TransformDirection(momentum)) * velocity);
			transform.Translate((Vector3.forward * velocity) + (transform.InverseTransformDirection(momentum) * Time.deltaTime));
			//Debug.Log(momentum);
			momentum -= momentum * 0.9f * Time.deltaTime;
			//rigidbody.AddForce(Vector3.forward * velocity);

			if(enableCameraRollback)
			{
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
						if(transform.eulerAngles.z < 180)
						{
							backRotationDegrees *= -1;
						}

						transform.Rotate(0, 0, backRotationDegrees);
					}
				}
			}
		}
		catch(System.Exception e)
		{
			enableBalanceBoardControl = false;
		}
	}

    public void lockToTargetSpeed(float targetSpeed, float duration)
    {
        float startSpeed = speed;
        speedLocked = true;

        if (duration == 0) speed = targetSpeed;
        else StartCoroutine(adjustSpeed(targetSpeed, startSpeed, duration, true));
    }

    public void unlockSpeed(float duration)
    {
        float startSpeed = speed;
        StartCoroutine(adjustSpeed(defaultSpeed, startSpeed, duration, false));
    }

    IEnumerator adjustSpeed(float targetSpeed, float startSpeed, float duration, bool b)
    {
        yield return new WaitUntil(() => adjustSpeedToTargetSpeed(targetSpeed, startSpeed, duration));
        speedLocked = b;
    }

    private bool adjustSpeedToTargetSpeed(float targetSpeed, float startSpeed, float duration)
    {
        speed += ((targetSpeed - startSpeed) / duration) * Time.deltaTime;
        bool slowDown = (targetSpeed < startSpeed);

        if (speed <= targetSpeed && slowDown || speed >= targetSpeed && !slowDown)
        {
            speed = targetSpeed;
            return true;
        }
        else return false;
    }
}