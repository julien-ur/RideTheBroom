//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: For controlling in-game objects with tracked devices.
//
//=============================================================================

using UnityEngine;
using Valve.VR;

public class SteamVR_TrackedObject : MonoBehaviour
{
	public enum EIndex
	{
		None = -1,
		Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
		Device1,
		Device2,
		Device3,
		Device4,
		Device5,
		Device6,
		Device7,
		Device8,
		Device9,
		Device10,
		Device11,
		Device12,
		Device13,
		Device14,
		Device15
	}

	public EIndex index;

	[Tooltip("If not set, relative to parent")]
	public Transform origin;

    public bool isValid { get; private set; }

    private bool firstPos = true;
    private Quaternion rotCorr;
    private Vector3 posCorr;
    private Quaternion startRot;
    private Quaternion currRot;
    private float inputMultiplierVertical = 5f;
    private float inputMultiplierHorizontal = 5f;

    private void OnNewPoses(TrackedDevicePose_t[] poses)
	{
		if (index == EIndex.None)
			return;

		var i = (int)index;

        isValid = false;
		if (poses.Length <= i)
			return;

		if (!poses[i].bDeviceIsConnected)
			return;

		if (!poses[i].bPoseIsValid)
			return;

        isValid = true;

		var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);
        currRot = pose.rot;

        if (firstPos)
        {
            firstPos = false;
            startRot = pose.rot;

            Transform broom = GameObject.FindGameObjectWithTag("Broom").transform;
            rotCorr = Quaternion.Inverse(pose.rot) * broom.rotation;

            GameComponents.GetPlayer().GetComponentInChildren<PlayerCameraControl>().SetOffset(pose.pos);
        }

		if (false)//origin != null)
		{
			transform.position = origin.transform.TransformPoint(pose.pos);
			transform.rotation = origin.rotation * pose.rot;
		}
		else
		{
            Transform broom = GameObject.FindGameObjectWithTag("Broom").transform;
            broom.localRotation = pose.rot * rotCorr;

            //transform.localPosition = pose.pos - posCorr;
            //transform.rotation = pose.rot; // * rotCorr;
        }
	}

	SteamVR_Events.Action newPosesAction;

    SteamVR_TrackedObject()
	{
		newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
	}

	void OnEnable()
	{
		var render = SteamVR_Render.instance;
		if (render == null)
		{
			enabled = false;
			return;
		}

        newPosesAction.enabled = true;
	}

	void OnDisable()
	{
		newPosesAction.enabled = false;
		isValid = false;
	}

	public void SetDeviceIndex(int index)
	{
		if (System.Enum.IsDefined(typeof(EIndex), index))
			this.index = (EIndex)index;
	}

    public float GetAxis(string axis)
    {
        float axisInput = 0;

        if(axis == "Vertical")
        {
            //axisInput = Utilities.Remap(startRot.y - currRot.y, 55, -75, 1.0f, -1.0f);
            axisInput = Mathf.Clamp((startRot.x - currRot.x) * -inputMultiplierVertical, -1.0f, 1.0f);
        }
        else if (axis == "Horizontal")
        {
            //axisInput = Utilities.Remap(startRot.x - currRot.x, 7, -14, 1.0f, -1.0f);
            axisInput = Mathf.Clamp((startRot.z - currRot.z) * inputMultiplierHorizontal, -1.0f, 1.0f);
        }

        return axisInput;
    }
}