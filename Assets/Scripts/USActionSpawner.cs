using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class USActionSpawner : MonoBehaviour {

    public int CompleteCounterbalanceRepetitions = 3;
    public int TrainingActionRepetitions = 3;
    public float MinTimeBetweenActions = 5;
    public float MaxTimeBetweenActions = 20;

    public EventHandler ActionCountReached;

    private USAction _action;
    private List<List<USAction.TYPE>> _actionSequencePool;
    private List<USAction.TYPE> _trainingPool;
    private List<USAction.TYPE> _currentActionSequence;

    private bool _isTrainingRound;
    private int _trainingActionsToSpawn;
    private int _actionPoolRefillCount = 0;
    private int _actionCount = 0;
    private bool _actionFinished = false;


    void Start()
    {
        _action = GetComponent<USAction>();
        FillActionSequencePool();
        _currentActionSequence = TakeActionSequenceFromPool();

        if (!_isTrainingRound) return;
        _trainingPool = new List<USAction.TYPE>();
        FillTrainingPool();
    }

    private IEnumerator Spawner()
    {
        yield return new WaitForSeconds(Random.Range(15, 30));

        while (true)
        {
            yield return new WaitUntil(IsPlayerReady);
            StartNewAction();

            yield return new WaitUntil(() => _actionFinished);

            if ((_actionPoolRefillCount >= CompleteCounterbalanceRepetitions && _currentActionSequence.Count == 0) 
                || _isTrainingRound && _actionCount == _trainingPool.Count)
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
        _action.StartNewAction(GetNextActionFromPool(), _actionCount);
        _actionCount++;
    }

    private USAction.TYPE GetNextActionFromPool()
    {
        if (_isTrainingRound)
            return _trainingPool[_actionCount];

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

    private void FillTrainingPool()
    {
        for (var rep = 0; rep < Mathf.Ceil(TrainingActionRepetitions/2f); rep++)
        {
            Debug.Log(rep);
            for (var t = 0; t < _action.GetTypeCount(); t++)
            {
                int rest = (rep == Mathf.Floor(TrainingActionRepetitions/2f)) ? TrainingActionRepetitions % 2 : 0;
                for (var r = 0; r < 2 - rest; r++)
                {
                    _trainingPool.Add((USAction.TYPE)t);
                    Debug.Log(t);
                }
            }
        }
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

    public void StartSpawning(bool trainingRound = false)
    {
        _isTrainingRound = trainingRound;
        StartCoroutine(Spawner());
    }

    public void ResetActionCount()
    {
        _actionCount = 0;
    }
}
