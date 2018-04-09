using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Valve.VR;

public class USActionEventArgs : UserStudyControlEventArgs
{
    public USAction.TYPE ActionType { get; set; }
    public int Count { get; set; }
}

public class USAction : MonoBehaviour {

    public enum TYPE { Accelerate, RefillTank, POV }
    public float[] MaxCompletionTimes = { 5, 10, 10 };

    private List<List<TYPE>> _actionTypePermutations;

    public event EventHandler<USActionEventArgs> ActionStarted;
    public event EventHandler<USActionEventArgs> ActionSuccess;
    public event EventHandler<USActionEventArgs> ActionTimeOut;

    private PlayerControl _pc;
    private UserStudyControl _usc;
    private GameObject _actionItem;

    private TYPE _type;
    private Vector3 _playerStartFoward;
    private GameObject _refillTankObject;
    private GameObject _povContainer;
    private bool _refilledTank;
    private bool _admiredPovEnough;
    private bool _actionRunning;


    void Awake()
    {
        _actionTypePermutations = GenerateActionTypePermutations();
    }

    void Start()
    {
        _pc = GameComponents.GetPlayerControl();
        _usc = GameComponents.GetLevelControl().GetComponent<UserStudyControl>();
        _refillTankObject = _usc.GetRefillTankItem();
        _povContainer = _usc.GetPovContainer();

        GetComponent<USPlayerReactionDetection>().ReactionForTrigger += OnPlayerReactionForTrigger;
    }

    public void StartNewAction(TYPE t, int count)
    {
        StopAllCoroutines();
        var args = new USActionEventArgs { ActionType = t, Count = count };
        OnActionStarted(args);

        _type = t;
        Func<bool> condition = InitAndGetCondition();
        StartCoroutine(CheckFullfilment(condition, args));
    }

    private IEnumerator CheckFullfilment(Func<bool> condition, USActionEventArgs args)
    {
        float timer = 0;
        float maxTimeForCompletion = MaxCompletionTimes[(int)args.ActionType];

        while (!condition())
        {
            if ((timer += Time.deltaTime) > maxTimeForCompletion)
            {
                OnActionTimeOut(args);
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

        OnActionSuccess(args);
    }

    private bool HasPlayerSlowedDown()
    {
        return _pc.GetCurrentSpeed() >= _pc.GetMaxSpeed() - 1;
    }

    private bool HasPlayerRefilledTank()
    {
        return _refilledTank;
    }

    private bool HasPlayerSeenPov()
    {
        return _admiredPovEnough;
    }

    private Func<bool> InitAndGetCondition()
    {
        Func<bool> condition = null;

        if (_type == TYPE.Accelerate)
        {
            condition = HasPlayerSlowedDown;

        } else if (_type == TYPE.RefillTank) {

            _refilledTank = false;
            _playerStartFoward = _pc.transform.forward;
            condition = HasPlayerRefilledTank;

        } else if (_type == TYPE.POV) {

            _admiredPovEnough = false;
            SpawnPovAtSides();
            condition = HasPlayerSeenPov;
        }

        return condition;
    }

    private void SpawnPovAtSides()
    {
        Debug.Log("Spawn POV");
        Transform leftPovContainer = Instantiate(_povContainer).transform;
        Transform rightPovContainer = Instantiate(_povContainer).transform;

        _actionItem = new GameObject() { name = "PovsAtSides" };
        leftPovContainer.transform.parent = _actionItem.transform;
        rightPovContainer.transform.parent = _actionItem.transform;

        // move pov container to player
        Transform playerTrans = _pc.transform;
        Vector3 posWithoutSideAxes = playerTrans.position + (1 * playerTrans.up)
                                                          + (-32 * playerTrans.forward);
        Vector3 leftSideShift = (-43 * playerTrans.right);

        leftPovContainer.position = posWithoutSideAxes + leftSideShift;
        rightPovContainer.position = posWithoutSideAxes - leftSideShift;

        rightPovContainer.localScale = Vector3.Scale(rightPovContainer.localScale, new Vector3(-1, 1, 1));

        leftPovContainer.rotation = playerTrans.rotation;
        rightPovContainer.rotation = playerTrans.rotation;

        // ---------------------------------------------------------

        var selectables = rightPovContainer.GetComponentsInChildren<USPovSelectable>();
        foreach (USPovSelectable s in selectables)
        {
            s.Side = USPovSelectable.SIDE.Right;
        }

        var povControl = _actionItem.AddComponent<USPovControl>();
        povControl.SelectingSound = _usc.PovSelectingSound;
        USPovObject.PovAdmirationComplete += OnPlayerTriggeredItem;
    }

    private void SpawnFuelItem()
    {
        Debug.Log("Spawn Fuel");
        _actionItem = Instantiate(_refillTankObject);
        _actionItem.transform.position = _pc.transform.position - (30 * _playerStartFoward) + (10 * Vector3.up) + (5 * Vector3.right);

        ItemTrigger.PlayerTriggered += OnPlayerTriggeredItem;
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
        if (_actionItem && _type == TYPE.RefillTank) Destroy(_actionItem);

        if (ActionSuccess != null)
        {
            args.EventInfo = "Action Success";
            ActionSuccess(this, args);
        }
    }

    protected virtual void OnActionTimeOut(USActionEventArgs args)
    {
        _actionRunning = false;
        if (_actionItem) Destroy(_actionItem);

        if (ActionTimeOut != null)
        {
            args.EventInfo = "Action TimeOut";
            ActionTimeOut(this, args);
        }
    }

    public void OnPlayerReactionForTrigger(object sender, EventArgs args)
    {
        if (_type == TYPE.RefillTank)
        {
            SpawnFuelItem();
        }
    }

    public void OnPlayerTriggeredItem(object sender, ItemTriggerEventArgs args)
    {
        if (args.Item == ItemTrigger.ITEM.RefillTank)
        {
            _refilledTank = true;
        }
        else if (args.Item == ItemTrigger.ITEM.Pov)
        {
            _admiredPovEnough = true;
        }
    }

    private List<List<TYPE>> GenerateActionTypePermutations()
    {
        TYPE[] actionTypes = Enum.GetValues(typeof(TYPE)).Cast<TYPE>().ToArray();

        var result = new List<List<TYPE>>();

        foreach (TYPE[] actionOrder in Utilities.Permutations(actionTypes))
        {
            result.Add(actionOrder.ToList());
        }

        return result;
    }

    public List<List<TYPE>> GetAllPossibleActionTypeOrders()
    {
        return _actionTypePermutations;
    }

    public int GetTypeCount()
    {
        return Enum.GetNames(typeof(TYPE)).Length;
    }

    public bool IsActionRunning()
    {
        return _actionRunning;
    }

    public string GetCurrentActionName()
    {
        return _type.ToString();
    }
}
