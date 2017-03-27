using System;
using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Range(0.0f, 1.0f)] public float forceDrivenFactor = 0.5f;

    public float defaultSpeed = 20;
    [HideInInspector] public Vector3 momentum;

    public float rotationFactorX = 120;
    public float rotationFactorY = 120;

    public bool useBroomHardware = false;  // use gamepad instead of broom hardware, for testing purposes
    public bool useAndroidInput = true;
    public bool invertHorizontal = false;

    public AndroidInput androidInput;

    public bool enableBroomRollback = true;
    public float backRotationRate = 90; // degrees per second to roll back broom

    private Rigidbody rb;
    private PlayerCameraControl cameraControl;

    private float speed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraControl = GetComponentInChildren<PlayerCameraControl>();
        speed = 0;
    }

    void Update()
    {
        // non physical forward drive component
        // can't set rigidbody velocity here, as it would override the calculated velocity 
        // from the addForce method of the physical forward drive component
        transform.Translate(Vector3.forward * speed * (1 - forceDrivenFactor) * Time.deltaTime);

        // physical forward drive component
        rb.AddForce(transform.forward * speed * forceDrivenFactor);

        // fake physical momentum, used for windzones
        transform.Translate(momentum * Time.deltaTime, Space.World);
        momentum -= (momentum / Constants.WINDZONE_MOMENTUM_LOSS_TIME) * Time.deltaTime;

        // Rotate broom based on input
        float inputVertical = 0;
        float inputHorizontal = 0;

        if (useBroomHardware)
        {
            if(useAndroidInput)
            {
                inputVertical = androidInput.GetAxis("Vertical");
                inputHorizontal = androidInput.GetAxis("Horizontal");
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

        float rotateX = inputVertical * rotationFactorX * Time.deltaTime;
        float rotateY = inputHorizontal * rotationFactorY * Time.deltaTime * -1;

        // rotate broom horizontally
        transform.RotateAround(transform.position, transform.right, rotateX);

        // rotate broom vertically
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
        changeSpeedToTargetSpeed(defaultSpeed, 2);
    }

    public void changeSpeedToTargetSpeed(float targetSpeed, float duration)
    {
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
        float startSpeed = speed;
        bool isSlowDown = (targetSpeed < startSpeed);

        while (speed >= targetSpeed && isSlowDown || speed <= targetSpeed && !isSlowDown)
        {
            speed += ((targetSpeed - startSpeed) / duration) * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        speed = targetSpeed;
    }
}