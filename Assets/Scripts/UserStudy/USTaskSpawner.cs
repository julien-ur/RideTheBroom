using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class USTaskSpawner : MonoBehaviour {

    public EventHandler ActionCountReached;

    public float MinTimeBetweenActions = 5;
    public float MaxTimeBetweenActions = 7;
    public float MinTimeBetweenRings = 2;
    public float MaxTimeBetweenRings = 4;
    public float TimeBetweenPovs = 6;
    public float TimeBetweenRingAndPov = 2;

    private USTaskPoolGenerator _poolGenerator;
    private USTaskController _taskControl;
    private List<PoolItem> _actionPool;

    private int _spawnCount;
    private int _runningTaskCount;
    private bool _readyForNextSpawn;
    private bool _trainingRound;

    void Start()
    {
        _taskControl = GetComponent<USTaskController>();
        _poolGenerator = new USTaskPoolGenerator();

        _taskControl.TaskStarted += OnTaskStarted;
        _taskControl.TaskEnded += OnTaskEnded;
    }

    private IEnumerator Spawner()
    {
        // yield return new WaitForSeconds(Random.Range(15, 30));

        while (_actionPool.Count > 0)
        {
            yield return new WaitUntil(IsPlayerReady);

            PoolItem nextPoolItem = _actionPool[0];
            float waitDuration = MaxTimeBetweenRings;

            if (_actionPool.Count > 1)
            {
                PoolItem secondNextPoolItem = _actionPool[1];

                if (nextPoolItem.ContainsMainTask() && secondNextPoolItem.ContainsMainTask())
                {
                    waitDuration = Random.Range(MinTimeBetweenRings, MaxTimeBetweenRings);
                }
                if (nextPoolItem.ContainsMainTask() && secondNextPoolItem.ContainsSecondaryTask())
                {
                    waitDuration = TimeBetweenRingAndPov;
                }
                else if (nextPoolItem.ContainsSecondaryTask() && secondNextPoolItem.ContainsMainTask())
                {
                    waitDuration = MaxTimeBetweenRings - TimeBetweenRingAndPov;
                }
                else if (nextPoolItem.ContainsSecondaryTask() && secondNextPoolItem.ContainsSecondaryTask())
                {
                    waitDuration = TimeBetweenPovs;
                }
            }

            _actionPool.RemoveAt(0);
            StartTasks(nextPoolItem);
            _readyForNextSpawn = true;

            yield return new WaitForSeconds(waitDuration);
            yield return new WaitUntil(() => _readyForNextSpawn);
        }

        yield return new WaitForSeconds(1);
        OnActionCountReached();
    }

    private void StartTasks(PoolItem item)
    {
        _readyForNextSpawn = false;
        //_runningTaskCount = item.GetTaskCount();
        _taskControl.StartTasks(item, _spawnCount);
        _spawnCount++;
    }


    private bool IsPlayerReady()
    {
        return true;
    }

    private void OnTaskStarted(object sender, USTaskControllerEventArgs e)
    {
        if (e.Type == USTask.TYPE.Secondary)
            e.Task.GetTaskObject().GetComponent<USPovControl>().FirstContact += OnSecondaryTaskFirstActivated;
    }

    public void OnTaskEnded(object sender, USTaskControllerEventArgs args)
    {
        //if(--_runningTaskCount == 0)
        //    _readyForNextSpawn = true;

        if (args.Type == USTask.TYPE.Secondary)
        {
            _readyForNextSpawn = true;
            if (args.EventInfo == "timeout" && _trainingRound)
            {
                int newMainTasks = Random.Range(USTaskPoolGenerator.MinMainTasksOnlyBeforeSecondaryTask, USTaskPoolGenerator.MaxMainTasksOnlyBeforeSecondaryTask+1);
                for (int i = 0; i < newMainTasks; i++)
                {
                    _actionPool.Add(new PoolItem(USTask.POSITION.Middle, USTask.POSITION.None));
                }
                _actionPool.Add(new PoolItem(USTask.POSITION.Middle, args.Position));
            }
        }
    }

    protected virtual void OnActionCountReached()
    {
        if (ActionCountReached != null)
            ActionCountReached(this, EventArgs.Empty);
    }

    public void OnSecondaryTaskFirstActivated(object sender, EventArgs args)
    {
        _readyForNextSpawn = false;
    }

    public void StartSpawning()
    {
        StartCoroutine(Spawner());
    }

    public void InitNewRound(bool trainingRound)
    {
        _spawnCount = 0;
        _trainingRound = trainingRound;
        _actionPool = _poolGenerator.GeneratePool(false);
    }
}
