using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class USActionSpawner : MonoBehaviour {

    public EventHandler ActionCountReached;

    public float MinTimeBetweenActions = 3;
    public float MaxTimeBetweenActions = 5;

    private USAction _action;
    private USActionPoolGenerator _poolGenerator;
    private List<PoolItem> _actionPool;

    private int _actionCount;
    private bool _actionFinished;

    void Start()
    {
        _action = GetComponent<USAction>();
        _poolGenerator = new USActionPoolGenerator();
    }

    private IEnumerator Spawner()
    {
        // yield return new WaitForSeconds(Random.Range(15, 30));

        while (_actionPool.Count > 0)
        {
            yield return new WaitUntil(IsPlayerReady);

            PoolItem nextPoolItem = _actionPool[0];
            _actionPool.RemoveAt(0);

            StartAction(nextPoolItem);

            yield return new WaitUntil(() => _actionFinished);
            yield return new WaitForSeconds(Random.Range(MinTimeBetweenActions, MaxTimeBetweenActions));
        }

        yield return new WaitForSeconds(1);
        OnActionCountReached();
    }

    private void StartAction(PoolItem item)
    {
        _actionFinished = false;
        _action.StartNewAction(item.MainTaskPos, item.SecondaryTaskPos, _actionCount);
        _actionCount++;
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

    public void InitNewRound(bool trainingRound)
    {
        _actionCount = 0;
        _actionPool = _poolGenerator.GeneratePool(trainingRound);
    }
}
