using System;
using System.Collections;
using UnityEngine;

public class PlayerRotationDetection : MonoBehaviour {

    public enum AXES { Horizontal, Vertical }
    public enum DIRECTION { LeftOrDown, RightOrUp, Both }

    IEnumerator CheckPlayerRot(AXES axes, DIRECTION direction, float minDetectionAngle, float predefinedStartAngle, Action callback)
    {
        Transform playerTrans = GameComponents.GetPlayer().transform;

        float startAngle = (predefinedStartAngle != 9999) ? predefinedStartAngle : (axes == AXES.Horizontal) ? playerTrans.rotation.eulerAngles.y : playerTrans.rotation.eulerAngles.x;

        while (true)
        {
            float currentAngle = (axes == AXES.Horizontal) ? playerTrans.rotation.eulerAngles.y : playerTrans.rotation.eulerAngles.x;
            float deltaAngle = Mathf.DeltaAngle(startAngle, currentAngle);
            //Debug.Log(deltaAngle +" " + axes +" " + direction +" " + minDetectionAngle);

            if (direction == DIRECTION.Both && Mathf.Abs(deltaAngle) > minDetectionAngle || 
                direction == DIRECTION.LeftOrDown && deltaAngle > minDetectionAngle ||
                direction == DIRECTION.RightOrUp && -deltaAngle > minDetectionAngle)
            {
                callback();
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public Coroutine StartDetectionForPlayerRotation(AXES axes, DIRECTION direction, float minDetectionAngle, Action callback, float startAngle=9999)
    {
        return StartCoroutine(CheckPlayerRot(axes, direction, minDetectionAngle, startAngle, callback));
    }
}
