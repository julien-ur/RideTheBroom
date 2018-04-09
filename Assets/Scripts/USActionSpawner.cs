using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class USActionSpawner : MonoBehaviour {

    public int CompleteCounterbalanceRepetitions = 3;
    public float MinTimeBetweenActions = 2;//5;
    public float MaxTimeBetweenActions = 5;//20;

    public EventHandler ActionCountReached;

    private USAction _action;
    private int _actionCount = 0;
    private List<List<USAction.TYPE>> _actionSequencePool;
    private List<USAction.TYPE> _currentActionSequence;
    private int _actionPoolRefillCount = 0;
    private bool _actionFinished = false;

    void Start()
    {
        _action = GetComponent<USAction>();
        FillActionSequencePool();
        _currentActionSequence = TakeActionSequenceFromPool();
    }

    private IEnumerator Spawner()
    {
        yield return new WaitForSeconds(Random.Range(MinTimeBetweenActions, MaxTimeBetweenActions));

        while (true)
        {
            yield return new WaitUntil(IsPlayerReady);
            StartNewAction();

            yield return new WaitUntil(() => _actionFinished);

            if (_actionPoolRefillCount >= CompleteCounterbalanceRepetitions && _currentActionSequence.Count == 0)
            {
                yield return new WaitForSeconds(1);
                OnActionCountReached();
                yield break;
            }

            yield return new WaitForSeconds(Random.Range(MinTimeBetweenActions, MaxTimeBetweenActions));
        }
    }

    private void StartNewAction()
    {
        _actionFinished = false;
        _action.StartNewAction(USAction.TYPE.POV, _actionCount);
        _actionCount++;
    }

    private USAction.TYPE GetNextActionFromPool()
    {
        if (_actionSequencePool.Count == 0)
        {
            FillActionSequencePool();
            _actionPoolRefillCount++;
        }

        if (_currentActionSequence.Count == 0)
            _currentActionSequence = TakeActionSequenceFromPool();

        USAction.TYPE nextAction = _currentActionSequence[0];
        _currentActionSequence.RemoveAt(0);

        return nextAction;
    }

    private List<USAction.TYPE> TakeActionSequenceFromPool()
    {
        int rndIndex = Random.Range(0, _actionSequencePool.Count - 1);
        var rndSequence = _actionSequencePool[rndIndex];
        _actionSequencePool.RemoveAt(rndIndex);
        return rndSequence;
    }

    private void FillActionSequencePool()
    {
        _actionSequencePool = _action.GetAllPossibleActionTypeOrders();
    }

    private bool IsPlayerReady()
    {
        return true;
    }

    public void OnActionFinished(object sender, USActionEventArgs args)
    {
        _actionFinished = true;
    }

    protected virtual void OnActionCountReached()
    {
        if (ActionCountReached != null)
            ActionCountReached(this, EventArgs.Empty);
    }

    public void StartSpawning()
    {
        StartCoroutine(Spawner());
    }

    public void ResetActionCount()
    {
        _actionCount = 0;
    }
}
