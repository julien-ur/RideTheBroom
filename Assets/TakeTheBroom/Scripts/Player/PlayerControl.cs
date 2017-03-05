using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VR;

public class PlayerControl : MonoBehaviour
{
    public float speedMin = 0.7f;
    public float speedMax = 15;
    public float defaultSpeed = 10;
    public float speedChange = 1;

    public float rotationFactorX = 60;
    public float rotationFactorY = 60;

    public bool useBroomHardware = false;  // use gamepad instead of broom hardware, for testing purposes
    public bool invertHorizontal = false;

    public bool enableBroomRollback = true;
    public float backRotationRate = 90; // degrees per second to roll back broom

    private Rigidbody rb;
    private PlayerCameraControl cameraControl;

    private float speed;
    private bool speedLocked = false;

    [HideInInspector] public Vector3 momentum;
    private float momentumLossInSec = 2.0f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraControl = GetComponentInChildren<PlayerCameraControl>();
        speed = defaultSpeed;
    }

    void Update()
    {
        // broom forward drive
        rb.velocity = ((transform.forward * speed * Time.deltaTime) + (momentum * Time.deltaTime)) * 100;
        momentum -= momentum / momentumLossInSec * Time.deltaTime;

        // Rotate broom based on input
        float inputVertical = 0;
        float inputHorizontal = 0;

        if (useBroomHardware)
        {
            inputHorizontal = BroomHardwareInput.GetAxis("Vertical");
            inputHorizontal = BroomHardwareInput.GetAxis("Horizontal");
        }

        if (!useBroomHardware || BroomHardwareInput.HasThrownErrors())
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
        if (enableBroomRollback)
        {
            // back rotate brooms z Axis to zero degrees, when it was rotated
            if (transform.eulerAngles.z != 0) PerformBroomRollback();
        }

        // rotate player camera horizontally
        cameraControl.RollCamera(inputHorizontal);
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