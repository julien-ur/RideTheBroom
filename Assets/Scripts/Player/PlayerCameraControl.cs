using UnityEngine;

public class PlayerCameraControl : MonoBehaviour {

	public const float rotationRate = 60;       // degrees per second to roll camera horizontally in input direction
    public const float backRotationRate = 60;	// degrees per second to roll back camera to an upright postion, when no more horizontal input is made

	public void RollCamera (float inputHorizontal)
	{
        // if there is an horizontal input, roll camera on the z-Axis
        if (inputHorizontal != 0)
        {
            RotateCameraHorizontally(inputHorizontal);
        }
        else if (transform.localEulerAngles.z != 0) // roll back to zero degrees
        {
            RollBackCamera();
        }
	}

    private void RotateCameraHorizontally(float inputHorizontal)
    {
        float rotateZ = inputHorizontal * rotationRate * Time.deltaTime;

        // cap rotation at 45° in each direction
        // TODO: cap as variable?
        if (transform.eulerAngles.z < 45 || transform.eulerAngles.z > 315)
        {
            transform.Rotate(0, 0, rotateZ);
        }
    }

    private void RollBackCamera()
    {
        float backRotationDegrees = backRotationRate * Time.deltaTime;

        // if close to zero, set to zero
        // transform.eulerAngles.z = 0 does not work because of quaternion magic
        if (Mathf.Abs(backRotationDegrees) > Mathf.Abs(transform.eulerAngles.z))
        {
            transform.Rotate(0, 0, -transform.eulerAngles.z);
        }
        else // roll back at fixed rate
        {
            // find direction to avoid accidental 360° rolls (and vomit)
            if (transform.eulerAngles.z < 180) backRotationDegrees *= -1;

            transform.Rotate(0, 0, backRotationDegrees);
        }
    }
}