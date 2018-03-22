using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class USActionEventArgs : EventArgs
{
    public USAction.TYPE ActionType { get; set; }
    public int Count { get; set; }
}

public class USAction : MonoBehaviour {

    public enum TYPE { Accelerate, RefillTank, POV }

    public event EventHandler<USActionEventArgs> ActionStarted;
    public event EventHandler<USActionEventArgs> ActionSuccess;
    public event EventHandler<USActionEventArgs> ActionTimeOut;

    private PlayerControl _pc;
    private const float MaxTimeForCompletion = 5;

    public void Awake()
    {
        _pc = GameComponents.GetPlayerControl();
    }

    public void StartNewAction(TYPE t, int count)
    {
        Func<bool> condition = GetCondition(t);
        var args = new USActionEventArgs {ActionType = t, Count = count};

        InitSpecialPreconditions(t);

        OnActionStarted(args);
        StartCoroutine(CheckFullfilment(condition, args));
    }

    private IEnumerator CheckFullfilment(Func<bool> condition, USActionEventArgs args)
    {
        float timer = 0;

        while (!condition())
        {
            if ((timer += Time.deltaTime) > MaxTimeForCompletion)
            {
                OnActionTimeOut(args);
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

        OnActionSuccess(args);
    }

    private void SpawnFuelItem()
    {
        Debug.Log("Spawn Fuel");
    }

    private bool HasPlayerSlowedDown()
    {
        return _pc.GetCurrentSpeed() <= _pc.GetMinSpeed() + 1;
    }

    private bool HasPlayerRefilledTank()
    {
        return _pc.GetCurrentSpeed() <= _pc.GetMinSpeed() + 1;
    }

    private bool HasPlayerSeenPOV()
    {
        return _pc.GetCurrentSpeed() <= _pc.GetMinSpeed() + 1;
    }

    private Func<bool> GetCondition(TYPE t)
    {
        switch (t)
        {
            case TYPE.Accelerate:
                return HasPlayerSlowedDown;

            case TYPE.RefillTank:
                return HasPlayerRefilledTank;

            case TYPE.POV:
                return HasPlayerSeenPOV;
        }

        return null;
    }

    private void InitSpecialPreconditions(TYPE t)
    {
        switch (t)
        {
            case TYPE.Accelerate:
                break;

            case TYPE.RefillTank:
                SpawnFuelItem();
                break;

            case TYPE.POV:
                break;
        }
    }

    protected virtual void OnActionStarted(USActionEventArgs args)
    {
        if (ActionStarted != null)
            ActionStarted(this, args);
    }

    protected virtual void OnActionSuccess(USActionEventArgs args)
    {
        if (ActionSuccess != null)
            ActionSuccess(this, args);
    }

    protected virtual void OnActionTimeOut(USActionEventArgs args)
    {
        if (ActionTimeOut != null)
            ActionTimeOut(this, args);
    }

    public int GetTypeCount()
    {
        return Enum.GetNames(typeof(TYPE)).Length;
    }
}
