using System;
using System.Collections;
using UnityEngine;

public class Action : MonoBehaviour {

    public enum TYPE { SlowDown, RefillTank, POV }
    public TYPE type;

    private PlayerControl pc;
    private Func<bool> condition;
    private float maxTime = 5;

    public void StartAction()
    {
        pc = GameComponents.GetPlayerControl();

        switch (type)
        {
            case TYPE.SlowDown:
                condition = HasPlayerSlowedDown;
                break;

            case TYPE.RefillTank:
                break;

            case TYPE.POV:
                break;
        }
    }

    private bool HasPlayerSlowedDown()
    {
        return (pc.GetCurrentSpeed() < pc.GetMinSpeed());
    }

    IEnumerator ConditionChecker()
    {
        while (!condition())
        {
            yield return new WaitForEndOfFrame();
        }
    }
}
