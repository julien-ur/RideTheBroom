using System;
using UnityEngine;


public class USPlayerReactionDetection : MonoBehaviour {

    public enum PLAYER_REACTION { Detected, Save, ForTrigger }
    private float[] MinAccelerateDetectionAngles = { 30, 50, -1 };
    private float[] MinRefillTankDetectionAngles = { 40, 160, 60 };

    public EventHandler<UserStudyControlEventArgs> ReactionDetectedUnsave;
    public EventHandler<UserStudyControlEventArgs> ReactionDetectedSave;
    public EventHandler ReactionForTrigger;

    private PlayerRotationDetection _prd;
    private Coroutine[] detectionCoroutines;

    void Start()
    {
        _prd = GameComponents.GetPlayer().transform.GetComponent<PlayerRotationDetection>();
    }

    public void StartDetecting(USAction.TYPE actionType)
    {
        Coroutine unsave = null, forTrigger = null, save = null;

        PlayerRotationDetection.AXES axes;
        PlayerRotationDetection.DIRECTION direction;

        float detectedUnsaveThreshold;
        float detectedSaveThreshold;

        if (actionType == USAction.TYPE.Accelerate)
        {
            axes = PlayerRotationDetection.AXES.Vertical;
            direction = PlayerRotationDetection.DIRECTION.LeftOrDown;

            detectedUnsaveThreshold = MinAccelerateDetectionAngles[(int) PLAYER_REACTION.Detected];
            detectedSaveThreshold = MinAccelerateDetectionAngles[(int) PLAYER_REACTION.Save];

            unsave = _prd.StartDetectionForPlayerRotation(axes, direction, detectedUnsaveThreshold, OnReactionDetectedUnsave);
            save = _prd.StartDetectionForPlayerRotation(axes, direction, detectedSaveThreshold, OnReactionDetectedSave);
        }
        else if (actionType == USAction.TYPE.RefillTank)
        {
            axes = PlayerRotationDetection.AXES.Horizontal;
            direction = PlayerRotationDetection.DIRECTION.Both;

            detectedUnsaveThreshold = MinRefillTankDetectionAngles[(int) PLAYER_REACTION.Detected];
            detectedSaveThreshold = MinRefillTankDetectionAngles[(int) PLAYER_REACTION.Save];
            float detectedForTriggerThreshold = MinRefillTankDetectionAngles[(int)PLAYER_REACTION.ForTrigger];

            unsave = _prd.StartDetectionForPlayerRotation(axes, direction, detectedUnsaveThreshold, OnReactionDetectedUnsave);
            save = _prd.StartDetectionForPlayerRotation(axes, direction, detectedSaveThreshold, OnReactionDetectedSave);
            forTrigger = _prd.StartDetectionForPlayerRotation(axes, direction, detectedForTriggerThreshold, OnReactionForTrigger);
        }

        detectionCoroutines = new[] { unsave, forTrigger, save };
    }

    protected virtual void OnReactionDetectedUnsave()
    {
        Debug.Log("Unsave");
        if (ReactionDetectedUnsave != null)
            ReactionDetectedUnsave(this, new UserStudyControlEventArgs() { EventInfo = "ReactionDetectedUnsave" });
    }

    protected virtual void OnReactionForTrigger()
    {
        if (ReactionForTrigger != null)
            ReactionForTrigger(this, EventArgs.Empty);
    }

    protected virtual void OnReactionDetectedSave()
    {
        Debug.Log("Save");
        if (ReactionDetectedSave != null)
            ReactionDetectedSave(this, new UserStudyControlEventArgs() { EventInfo = "ReactionDetectedSave" });
    }

    public void OnActionStarted(object sender, USActionEventArgs eventArgs)
    {
        Debug.Log("Started detecting " + eventArgs.ActionType);
        StartDetecting(eventArgs.ActionType);
    }

    public void OnActionFinished(object sender, USActionEventArgs eventArgs)
    {
        foreach (var c in detectionCoroutines)
        {
            if (c != null) _prd.StopCoroutine(c);
        }
    }
}
