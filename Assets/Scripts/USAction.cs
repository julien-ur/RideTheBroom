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

    public enum TYPE { SlowDown, RefillTank, POV }

    public event EventHandler<USActionEventArgs> ActionSuccess;
    public event EventHandler<USActionEventArgs> ActionTimeOut;

    private PlayerControl _pc;
    private const float MaxTimeForCompletion = 10;

    public void Awake()
    {
        _pc = GameComponents.GetPlayerControl();
    }

    public void StartNewAction(TYPE t, int count)
    {
        Func<bool> condition = GetCondition(t);
        StartCoroutine(CheckFullfilment(t, condition, count));
    }


    private bool HasPlayerSlowedDown()
    {
        return _pc.GetCurrentSpeed() <= _pc.GetMinSpeed() + 1;
    }

    private IEnumerator CheckFullfilment(TYPE type, Func<bool> condition, int count)
    {
        float timer = 0;

        while (!condition())
        {
            if ((timer += Time.deltaTime) > MaxTimeForCompletion)
            {
                OnActionTimeOut(type, count);
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

        OnActionSuccess(type, count);
    }

    private Func<bool> GetCondition(TYPE t)
    {
        switch (t)
        {
            case TYPE.SlowDown:
                return HasPlayerSlowedDown;

            case TYPE.RefillTank:
                return HasPlayerSlowedDown;

            case TYPE.POV:
                return HasPlayerSlowedDown;
        }

        return null;
    }

    protected virtual void OnActionSuccess(TYPE type, int count)
    {
        if (ActionSuccess != null)
            ActionSuccess(this, new USActionEventArgs() { ActionType = type, Count = count });
    }

    protected virtual void OnActionTimeOut(TYPE type, int count)
    {
        if (ActionTimeOut != null)
            ActionTimeOut(this, new USActionEventArgs() { ActionType = type, Count = count });
    }

    public int GetTypeCount()
    {
        return Enum.GetNames(typeof(TYPE)).Length;
    }
}
