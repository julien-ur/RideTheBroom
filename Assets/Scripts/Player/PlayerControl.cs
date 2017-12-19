using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VR;

public class PlayerControl : MonoBehaviour
{
    [Range(0.0f, 1.0f)] public float forceDrivenFactor = 0.5f;

    public float defaultSpeed = 15;
    public float minSpeed = 5;
    public float maxSpeed = 25;

    [HideInInspector] public Vector3 momentum;
    public bool tiltAcceleration = true;

    public float rotationFactorX = 120;
    public float rotationFactorY = 120;

    public bool useBroomHardware = false;  // use gamepad instead of broom hardware, for testing purposes
    public bool useAndroidInput = false;
    public bool useViveTracker = true;
    public bool invertHorizontal = false;

    public AndroidInput androidInput;
    public SteamVR_TrackedObject viveTracker;

    public bool enableBroomRollback = true;
    public float backRotationRate = 90; // degrees per second to roll back broom

    private Rigidbody rb;
    private PlayerCameraControl cameraControl;

    private float speed;

    private float lastTime;
    private float time;

    private float lastAngle = 0;

    private float headStartYPos;
    private float maxHeadDelta = 0.7f;
    private float maxSpeedChangeFactor = 1.7f;

    public bool useAbsoluteAngle = false;
    private float lastAngleVertical;
    private float targetAngleVertical;
    public float timeToNextAngle = 0.1f;
    public float timePassedSinceLastAngleUpdate = 0;

    private bool isRotationEnabled = true;
    private bool adjustingSpeed = true;
    private bool speedTargetOutOfBounds = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraControl = GetComponentInChildren<PlayerCameraControl>();
        speed = 0;
        headStartYPos = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head).y;
    }

    void Update()
    {
        // VR leaning acceleration
        float vrAccellerationFactor = 1;

        //if(VRDevice.isPresent)
        //{
        //    float clampedHeadDelta = Mathf.Clamp(InputTracking.GetLocalPosition(VRNode.Head).y - headStartYPos, headStartYPos - maxHeadDelta, headStartYPos + maxHeadDelta);

        //    if (clampedHeadDelta > 0)
        //    {
        //        vrAccellerationFactor = Utilities.Remap(clampedHeadDelta, 0, maxHeadDelta, 1, 1/maxSpeedChangeFactor);
        //    }
        //    else
        //    {
        //        vrAccellerationFactor = Utilities.Remap(clampedHeadDelta, 0, -maxHeadDelta, 1, maxSpeedChangeFactor);
        //    }

        //    Debug.LogError("headStartYPos: " + headStartYPos + " actualYHeadPos: " + InputTracking.GetLocalPosition(VRNode.Head).y + " clampedHeadDelta: " + clampedHeadDelta + " vrAccellerationFactor: " + vrAccellerationFactor);
        //}

        // broom tilt acceleration
        float tiltAccelerationFactor = 1;
        if (tiltAcceleration)
        {
            float tiltAngle = transform.rotation.eulerAngles.x;
            tiltAngle = (tiltAngle > 180) ? tiltAngle - 360 : tiltAngle;
            tiltAccelerationFactor = Utilities.Remap(tiltAngle, -45, 45, -0.1f, 0.3f);
        }
        if (speed >= minSpeed && speed <= maxSpeed) speed += tiltAccelerationFactor;

        if (!adjustingSpeed && !speedTargetOutOfBounds) speed = Mathf.Max(Mathf.Min(speed, maxSpeed), minSpeed);

        // non physical forward drive component
        // can't set rigidbody velocity here, as it would override the calculated velocity 
        // from the addForce method of the physical forward drive component
        transform.Translate(Vector3.forward * speed * (1 - forceDrivenFactor) * vrAccellerationFactor * Time.deltaTime);

        // physical forward drive component
        rb.AddForce(transform.forward * speed * forceDrivenFactor * vrAccellerationFactor);

        // fake physical momentum, used for windzones
        transform.Translate(momentum * Time.deltaTime, Space.World);
        momentum -= (momentum / Constants.WINDZONE_MOMENTUM_LOSS_TIME) * Time.deltaTime;

        // rotate broom based on input
        float inputVertical = 0;
        float inputHorizontal = 0;

        if (useBroomHardware)
        {
            if(useAndroidInput)
            {
                inputVertical = androidInput.GetAxis("Vertical");
                inputHorizontal = androidInput.GetAxis("Horizontal");
            }
            else if(useViveTracker)
            {
                inputVertical = viveTracker.GetAxis("Vertical");
                inputHorizontal = viveTracker.GetAxis("Horizontal");
            }
            else
            {
                inputVertical = BroomHardwareInput.GetAxis("Vertical");
                inputHorizontal = BroomHardwareInput.GetAxis("Horizontal");
            }
        }

        if (!useBroomHardware) // || BroomHardwareInput.HasThrownErrors())
        {
            inputVertical = -Input.GetAxis("Vertical");
            inputHorizontal = -Input.GetAxis("Horizontal");

            if (invertHorizontal) inputHorizontal *= -1;
        }

        if(isRotationEnabled || !UnityEngine.XR.XRDevice.isPresent)
        {

            float rotateX = inputVertical * rotationFactorX * Time.deltaTime;
            float rotateY = inputHorizontal * rotationFactorY * Time.deltaTime * -1;

            // rotate broom horizonatlly
            if(useAbsoluteAngle)
            {
                //float absoluteAngleY = androidInput.getAngleVertical();

                Vector3 tempAngles = transform.localEulerAngles;

                timePassedSinceLastAngleUpdate += Time.deltaTime;

                if(targetAngleVertical != androidInput.getAngleVertical())
                {
                    lastAngleVertical = tempAngles.x;
                    targetAngleVertical = androidInput.getAngleVertical();
                    timePassedSinceLastAngleUpdate = 0;
                }

                tempAngles.x = Mathf.LerpAngle(lastAngleVertical, targetAngleVertical, (timePassedSinceLastAngleUpdate / timeToNextAngle));

                //Debug.Log(tempAngles.x);
                //tempAngles.y = androidInput.getAngleHorizontal() * 3;
                transform.localEulerAngles = tempAngles;
            }
            else
            {
                // rotate broom vertically
                transform.RotateAround(transform.position, transform.right, rotateX);
            }

            if (enableBroomRollback)
                transform.RotateAround(transform.position, Vector3.up, rotateY);
            else
                transform.RotateAround(transform.position, transform.up, rotateY);
        

            // prevent overhead flying if broom rollback is enabled
            if (enableBroomRollback && Mathf.Abs(inputVertical) < 0.1)
            {
                // back rotate brooms z Axis to zero degrees, when it was rotated
                if (transform.eulerAngles.z != 0) PerformBroomRollback();

                // rotate player camera horizontally
                //cameraControl.RollCamera(inputHorizontal);
            }
        }

		if(false)//UnityEngine.XR.XRDevice.isPresent)
        {
			
			Transform broomTransform = transform.parent.Find("Broom").transform;
            Vector3 pos = broomTransform.localPosition;
            pos.x = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head).x;
            broomTransform.localPosition = pos;
        }

        //lastTime = time;
        //time = Time.realtimeSinceStartup;
        //Debug.Log("time: " + (time-lastTime));
    }

    private void PerformBroomRollback()
    {
        float backRotationDegrees = backRotationRate * Time.deltaTime;

        // if close to zero, set to zero
        // transform.eulerAngles.z = 0 does not work because of unity rotation order z > x > y (z affects the other two dimensions)
        if (Mathf.Abs(backRotationDegrees) > Mathf.Abs(transform.eulerAngles.z))
        {
            transform.Rotate(0, 0, -transform.eulerAngles.z);
        }
        else // roll back at fixed rate
        {
            // find direction to avoid accidental 360° rolls (and vomit)
            if (transform.eulerAngles.z < 180)
            {
                backRotationDegrees *= -1;
            }

            transform.Rotate(0, 0, backRotationDegrees);
        }
    }

    public void startBroom()
    {
        Debug.Log("vroom vroom");
        changeSpeedToTargetSpeed(defaultSpeed, 2);
    }

    public void changeSpeedToTargetSpeed(float targetSpeed, float duration)
    {
        if (targetSpeed > maxSpeed || targetSpeed < minSpeed) {
            speedTargetOutOfBounds = true;
        } else
        {
            speedTargetOutOfBounds = false;
        }

        if (duration == 0)
            speed = targetSpeed;
        else
            StartCoroutine(adjustSpeed(targetSpeed, duration));
    }

    public void changeSpeedToDefaultSpeed(float duration)
    {
        StartCoroutine(adjustSpeed(defaultSpeed, duration));
    }

    IEnumerator adjustSpeed(float targetSpeed, float duration)
    {
        adjustingSpeed = true;

        float startSpeed = speed;
        bool isSlowDown = (targetSpeed < startSpeed);

        while (speed >= targetSpeed && isSlowDown || speed <= targetSpeed && !isSlowDown)
        {
            speed += ((targetSpeed - startSpeed) / duration) * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        speed = targetSpeed;

        adjustingSpeed = false;
    }

    public void changeSpeed(float targetSpeed)
    {
        defaultSpeed = targetSpeed;
    }

    public void EnableRotation()
    {
        isRotationEnabled = true;
        GetComponent<SteamVR_TrackedObject>().enabled = true;
    }

    public void DisableRotation()
    {
        isRotationEnabled = false;
        GetComponent<SteamVR_TrackedObject>().enabled = false;
    }

    public float getCurrentSpeed()
    {
        return speed;
    }

    public float getMaxSpeed()
    {
        return maxSpeed;
    }

    
}