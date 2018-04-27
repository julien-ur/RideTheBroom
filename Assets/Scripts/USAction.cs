using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class USActionEventArgs : UserStudyControlEventArgs
{
    public USAction.POSITION MainTaskPos { get; set; }
    public USAction.POSITION SecondaryTaskPos { get; set; }
    public int Count { get; set; }
}

public class USAction : MonoBehaviour {

    public enum TYPE { MainOnly, SecondaryOnly, MainAndSecondary }
    public enum POSITION { Right, Middle, Left, None=-1 }
    public enum RELATION { Synchronous, Asynchronous, None=-1 }

    public float MaxCompletionTimeMainTask = 5;
    public float MaxCompletionTimeSecondaryTask = 7;

    public event EventHandler<USActionEventArgs> ActionStarted;
    public event EventHandler<USActionEventArgs> ActionSuccess;
    public event EventHandler<USActionEventArgs> ActionTimeOut;

    private UserStudyControl _usc;
    private Transform _playerTrans;

    private GameObject _povObject;
    private GameObject _ringObject;
    private GameObject _ringInactiveObject;

    private GameObject _actionRings;
    private GameObject _actionPov;
    private USActionEventArgs _currentArgs;
    private bool _actionExecuted;
    private bool _actionRunning;


    void Start()
    {
        _usc = GameComponents.GetLevelControl().GetComponent<UserStudyControl>();
        _playerTrans = GameComponents.GetPlayer().transform;

        _ringObject = _usc.GetRingObject();
        _ringInactiveObject = _usc.GetRingInactiveObject();
        _povObject = _usc.GetPovContainer();

        ItemTrigger.PlayerTriggered += OnPlayerTriggeredItem;
    }

    public void StartNewAction(POSITION mainTaskPos, POSITION secondaryTaskPos, int count)
    {
        StopAllCoroutines();
        _actionExecuted = false;

        if (mainTaskPos != POSITION.None) SpawnRings(mainTaskPos);
        if (secondaryTaskPos != POSITION.None) SpawnPov(secondaryTaskPos);

        _currentArgs = new USActionEventArgs { MainTaskPos = mainTaskPos, SecondaryTaskPos = secondaryTaskPos, Count = count };
        
        StartCoroutine(CheckFullfilment(_currentArgs));
        OnActionStarted(_currentArgs);
    }

    private IEnumerator CheckFullfilment(USActionEventArgs args)
    {
        float timer = 0;
        float maxCompletionTime = args.SecondaryTaskPos == POSITION.None
            ? MaxCompletionTimeMainTask
            : MaxCompletionTimeSecondaryTask;

        while (!_actionExecuted)
        {
            if ((timer += Time.deltaTime) > maxCompletionTime)
            {
                OnActionTimeOut(args);
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }

        OnActionSuccess(args);
    }

    private void SpawnRings(POSITION activePos)
    {
        _actionRings = new GameObject() { name = "ActionRings" };
        var positions = GetPositionsExcluding(new[] { POSITION.None });

        foreach (POSITION pos in positions)
        {
            Transform ringTrans = Instantiate(pos == activePos ? _ringObject : _ringInactiveObject).transform;
            ringTrans.position = _playerTrans.position + 40 * _playerTrans.forward;

            int sideShift = 15;
            if (pos == POSITION.Left)
                ringTrans.position -= sideShift * _playerTrans.right;

            else if (pos == POSITION.Right)
                ringTrans.position += sideShift * _playerTrans.right;

            ringTrans.localRotation = _playerTrans.rotation;
            ringTrans.Rotate(new Vector3(-90, 0, 0));

            ringTrans.parent = _actionRings.transform;
        }
    }

    private void SpawnPov(POSITION activePos)
    {
        Transform povContainerTrans = Instantiate(_povObject).transform;

        _actionPov = new GameObject() { name = "ActionPov" };
        povContainerTrans.parent = _actionPov.transform;

        var povControl = _actionPov.AddComponent<USPovControl>();
        povControl.SelectingSound = _usc.PovSelectingSound;
        povControl.SetPos(activePos);
        USPovObject.PovAdmirationComplete += OnPlayerTriggeredItem;
    }

    protected virtual void OnActionStarted(USActionEventArgs args)
    {
        _actionRunning = true;

        if (ActionStarted != null)
        {
            args.EventInfo = "Action Started";
            ActionStarted(this, args);
        }
    }

    protected virtual void OnActionSuccess(USActionEventArgs args)
    {
        _actionRunning = false;

        if (ActionSuccess != null)
        {
            args.EventInfo = "Action Success";
            ActionSuccess(this, args);
        }
    }

    protected virtual void OnActionTimeOut(USActionEventArgs args)
    {
        _actionRunning = false;
        if (_actionPov) Destroy(_actionPov);
        if (_actionRings) Destroy(_actionRings);

        if (ActionTimeOut != null)
        {
            args.EventInfo = "Action TimeOut";
            ActionTimeOut(this, args);
        }
    }

    public void OnPlayerTriggeredItem(object sender, ItemTriggerEventArgs args)
    {
        if (args.Item == ItemTrigger.ITEM.Pov || 
            (args.Item == ItemTrigger.ITEM.Ring && _currentArgs.SecondaryTaskPos == POSITION.None))
        {
            _actionExecuted = true;
        }
    }

    public bool IsActionRunning()
    {
        return _actionRunning;
    }

    public static POSITION[] GetPositionsExcluding(POSITION[] positionsToExclude)
    {
        return Enum.GetValues(typeof(POSITION))
            .Cast<POSITION>()
            .Where(pos => !positionsToExclude.Contains(pos))
            .ToArray();
    }

    public string GetCurrentMainTaskPosition()
    {
        if (!_actionRunning || _currentArgs.MainTaskPos == POSITION.None)
            return null;

        return _currentArgs.MainTaskPos.ToString();
    }

    public string GetCurrentSecondaryTaskPosition()
    {
        if (!_actionRunning || _currentArgs.SecondaryTaskPos == POSITION.None)
            return null;

        return _currentArgs.SecondaryTaskPos.ToString();
    }
}
