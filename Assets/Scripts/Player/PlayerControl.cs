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
    public bool spaceTiltAcceleration = false;

    public float rotationFactorX = 120;
    public float rotationFactorY = 120;
    public bool noRotationBlocking = false;
    public bool blockHorizontalRotation;
    public bool blockVerticalRotation;

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

    private Vector3 startRotation;
    private Vector3 rotationScopeCenter;
    private float headStartYPos;
    private float maxHeadDelta = 0.7f;
    private float maxSpeedChangeFactor = 1.7f;

    public bool useAbsoluteAngle = false;
    private float lastAngleVertical;
    private float targetAngleVertical;
    public float timeToNextAngle = 0.1f;
    public float timePassedSinceLastAngleUpdate = 0;

    private bool isRotationEnabled = true;
    private bool horizontalRotationLimited;
    private bool verticalRotationLimited;
    private float verticalRotationLimit = 180;
    private float horizontalRotationLimit = 180;
    private bool adjustingSpeed = true;
    private bool speedTargetOutOfBounds = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraControl = GetComponentInChildren<PlayerCameraControl>();
        speed = 0;
        headStartYPos = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head).y;
        startRotation = rotationScopeCenter = transform.rotation.eulerAngles;
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

        // broom tilt acceleration
        float tiltAccelerationFactor = 0;
        if (tiltAcceleration)
        {
            float tiltAngle = transform.localRotation.eulerAngles.x;
            tiltAngle = (tiltAngle > 180) ? tiltAngle - 360 : tiltAngle;
            tiltAccelerationFactor = Utilities.Remap(tiltAngle, -45, 45, -0.3f, 0.3f);
        }
        else if (spaceTiltAcceleration)
        {
            tiltAccelerationFactor = inputVertical * 2f;
        }

        // non physical forward drive component
        // can't set rigidbody velocity here, as it would override the calculated velocity 
        // from the addForce method of the physical forward drive component
        transform.Translate(Vector3.forward * speed * (1 - forceDrivenFactor) * vrAccellerationFactor * Time.deltaTime);

        // physical forward drive component
        rb.AddForce(transform.forward * speed * forceDrivenFactor * vrAccellerationFactor);

        // fake physical momentum, used for windzones
        transform.Translate(momentum * Time.deltaTime, Space.World);
        momentum -= (momentum / Constants.WINDZONE_MOMENTUM_LOSS_TIME) * Time.deltaTime;

        if (speed >= minSpeed && speed <= maxSpeed) speed += tiltAccelerationFactor;

        if (!adjustingSpeed && !speedTargetOutOfBounds)
        {
            speed = Mathf.Max(Mathf.Min(speed, maxSpeed), minSpeed);
            if (speed > defaultSpeed && Mathf.Abs(tiltAccelerationFactor) < 0.06f) speed -= (spaceTiltAcceleration) ? 0.06f : 0.03f;
        }

        if (isRotationEnabled || !UnityEngine.XR.XRDevice.isPresent || noRotationBlocking)
        {

            float rotateX = 0;
            float rotateY = 0;

            if (noRotationBlocking || !(blockVerticalRotation || verticalRotationLimited &&
                  (inputVertical < 0 && Mathf.DeltaAngle(transform.rotation.eulerAngles.x, rotationScopeCenter.x) >= verticalRotationLimit ||
                  inputVertical > 0 && Mathf.DeltaAngle(transform.rotation.eulerAngles.x, rotationScopeCenter.x) <= -verticalRotationLimit)))
            {
                rotateX = inputVertical * rotationFactorX * Time.deltaTime;
            }
            if (noRotationBlocking || !(blockHorizontalRotation || horizontalRotationLimited &&
                  (inputHorizontal > 0 && Mathf.DeltaAngle(transform.rotation.eulerAngles.y, rotationScopeCenter.y) >= horizontalRotationLimit ||
                  inputHorizontal < 0 && Mathf.DeltaAngle(transform.rotation.eulerAngles.y, rotationScopeCenter.y) <= -horizontalRotationLimit)))
            {
                rotateY = inputHorizontal * rotationFactorY * Time.deltaTime * -1;
            }

            // rotate broom horizonatlly
            if (useAbsoluteAngle)
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

    public void StartBroom()
    {
        // Debug.Log("vroom vroom");
        ChangeSpeedToTargetSpeed(defaultSpeed, 2);
    }

    public void ChangeSpeedToTargetSpeed(float targetSpeed, float duration)
    {
        StopAllCoroutines();

        if (targetSpeed > maxSpeed || targetSpeed < minSpeed) {
            speedTargetOutOfBounds = true;
        } else
        {
            speedTargetOutOfBounds = false;
        }
        if (duration == 0)
            speed = targetSpeed;
        else
            StartCoroutine(AdjustSpeed(targetSpeed, duration));
    }

    public void ChangeSpeedToDefaultSpeed(float duration)
    {
        speedTargetOutOfBounds = false;
        StartCoroutine(AdjustSpeed(defaultSpeed, duration));
    }

    private IEnumerator AdjustSpeed(float targetSpeed, float duration)
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

    private IEnumerator BlockRotation(string blockedAxes, bool withXRollback, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (blockedAxes.Contains("x")) blockVerticalRotation = verticalRotationLimited = true;
        if (blockedAxes.Contains("y")) blockHorizontalRotation = horizontalRotationLimited = true;
        if (withXRollback) StartCoroutine(RollBackToStartRotation("x"));
    }

    private IEnumerator RollBackToStartRotation(string axis)
    {
        float backRotationDegrees = Time.deltaTime * 50;

        int xFactor = 1;
        int yFactor = 1;
        if (transform.rotation.eulerAngles.x < 180) xFactor = -1;
        if (transform.rotation.eulerAngles.y < 180) yFactor = -1;
        if (axis.Contains("x")) xFactor = 0;
        if (axis.Contains("y")) yFactor = 0;

        while (xFactor != 0 || yFactor != 0)
        {
            if (xFactor != 0 && Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.x, startRotation.x)) <
                Mathf.Abs(backRotationDegrees))
            {
                transform.Rotate(startRotation.x - transform.rotation.eulerAngles.x, 0, 0);
                xFactor = 0;
            }

            if (yFactor != 0 && Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.y, startRotation.y)) <
                Mathf.Abs(backRotationDegrees))
            {
                transform.Rotate(0, startRotation.y - transform.rotation.eulerAngles.y, 0);
                yFactor = 0;
            }

            transform.Rotate(backRotationDegrees * xFactor, backRotationDegrees * yFactor, 0);
            yield return new WaitForEndOfFrame();
        }
    }

    public void ChangeSpeed(float targetSpeed)
    {
        defaultSpeed = targetSpeed;
        minSpeed = targetSpeed - 4;
        maxSpeed = targetSpeed + 10;
    }

    public void ChangeSpeed(float target, float min, float max)
    {
        defaultSpeed = target;
        minSpeed = min;
        maxSpeed = max;
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

    public void BlockRotationForAxis(string blockedAxes, bool withXRoolback=false, float delay=0)
    {
        StartCoroutine(BlockRotation(blockedAxes, withXRoolback, delay));
    }

    public void RemoveRotationLimitFromAxes(string freeAxes)
    {
        if (freeAxes.Contains("x")) blockVerticalRotation = verticalRotationLimited = false;
        if (freeAxes.Contains("y")) blockHorizontalRotation = horizontalRotationLimited = false;
    }

    public void LimitRotationScopeByAxis(char axis, float halfDegrees)
    {
        if (axis == 'x')
        {
            verticalRotationLimited = (halfDegrees < 180);
            blockVerticalRotation = (halfDegrees == 0);
            verticalRotationLimit = halfDegrees;
        }
        else if (axis == 'y')
        {
            horizontalRotationLimited = (halfDegrees < 180);
            blockHorizontalRotation = (halfDegrees == 0);
            horizontalRotationLimit = halfDegrees;
        }
    }

    public void UpdateRotationScopeCenter()
    {
        rotationScopeCenter = transform.rotation.eulerAngles;
    }

    public float GetCurrentSpeed()
    {
        return speed + rb.velocity.z;
    }

    public float GetMinSpeed()
    {
        return minSpeed;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }
}