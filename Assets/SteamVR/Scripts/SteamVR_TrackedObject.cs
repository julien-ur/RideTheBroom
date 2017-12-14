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
    private Vector3 startRot;
    private Vector3 currRot;
    private float inputDivisorHorizontal = 10.0f;
    private float inputDivisorVertical = 45.0f;

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
        currRot = pose.rot.eulerAngles;

        if (firstPos)
        {
            firstPos = false;
            startRot = pose.rot.eulerAngles;
            rotCorr = pose.rot * Quaternion.Inverse(origin.rotation);
            posCorr = pose.pos - origin.position;
            GameComponents.GetPlayer().GetComponentInChildren<PlayerCameraControl>().SetOffset(posCorr);
        }

		if (false)//origin != null)
		{
			transform.position = origin.transform.TransformPoint(pose.pos);
			transform.rotation = origin.rotation * pose.rot;
		}
		else
		{
			// transform.localPosition = pose.pos - posCorr;
			// transform.localRotation = pose.rot * rotCorr;
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
            axisInput = Mathf.Clamp((startRot.y - currRot.y) / inputDivisorVertical, -1.0f, 1.0f);
        }
        else if (axis == "Horizontal")
        {
            //axisInput = Utilities.Remap(startRot.x - currRot.x, 7, -14, 1.0f, -1.0f);
            axisInput = Mathf.Clamp((startRot.x - currRot.x) / inputDivisorHorizontal, -1.0f, 1.0f);
        }

        return axisInput;
    }
}