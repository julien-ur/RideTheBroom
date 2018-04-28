using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class USTaskSpawner : MonoBehaviour {

    public EventHandler ActionCountReached;

    public float MinTimeBetweenActions = 3;
    public float MaxTimeBetweenActions = 5;

    private USTaskPoolGenerator _poolGenerator;
    private USTaskController _taskControl;
    private List<PoolItem> _actionPool;

    private int _spawnCount;
    private int _runningTaskCount;
    private bool _readyForNextSpawn;

    void Start()
    {
        _taskControl = GetComponent<USTaskController>();
        _poolGenerator = new USTaskPoolGenerator();

        _taskControl.TaskEnded += OnTaskEnded;
    }

    private IEnumerator Spawner()
    {
        // yield return new WaitForSeconds(Random.Range(15, 30));

        while (_actionPool.Count > 0)
        {
            yield return new WaitUntil(IsPlayerReady);

            PoolItem nextPoolItem = _actionPool[0];
            _actionPool.RemoveAt(0);

            StartTasks(nextPoolItem);

            yield return new WaitUntil(() => _readyForNextSpawn);
            yield return new WaitForSeconds(Random.Range(MinTimeBetweenActions, MaxTimeBetweenActions));
        }

        yield return new WaitForSeconds(1);
        OnActionCountReached();
    }

    private void StartTasks(PoolItem item)
    {
        _readyForNextSpawn = false;
        _runningTaskCount = item.GetTaskCount();
        _taskControl.StartTasks(item, _spawnCount);
        _spawnCount++;
    }

    private bool IsPlayerReady()
    {
        return true;
    }

    public void OnTaskEnded(object sender, USTaskControllerEventArgs args)
    {
        _runningTaskCount--;

        if(_runningTaskCount == 0)
            _readyForNextSpawn = true;
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
        _spawnCount = 0;
        _actionPool = _poolGenerator.GeneratePool(trainingRound);
    }
}
