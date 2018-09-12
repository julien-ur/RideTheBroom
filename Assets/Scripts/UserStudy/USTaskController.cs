using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USTaskControllerEventArgs : EventArgs, ICloneable
{
    public USTask.TYPE Type;
    public USTask.POSITION Position;
    public int SpawnCount;
    public string EventInfo;
    public USTask Task;

    public object Clone()
    {
        return MemberwiseClone();
    }
}

public class USTaskController : MonoBehaviour
{
    public EventHandler<USTaskControllerEventArgs> TaskSpawned;
    public EventHandler<USTaskControllerEventArgs> TaskStarted;
    public EventHandler<USTaskControllerEventArgs> TaskEnded;

    private UserStudyControl _usc;
    //private FeedbackServer _fbs;
    private FeedbackUSB _feedbackUSB;
    private ScoreDisplayControl _scc;
    private AudioSource _audioSource;

    private const float MainTaskActivationDelay = 0f;
    private const float AudioDelay = 1.28f;

    private AudioClip _timeOutSound;
    private AudioClip _successSound;
    private float _timeOutVolume;
    private float _successVolume;

    void Awake()
    {
        _usc = GameComponents.GetUserStudyControl();
        //_fbs = GameComponents.GetGameController().GetComponent<FeedbackServer>();
        _feedbackUSB = GameComponents.GetGameController().GetComponent<FeedbackUSB>();
        _scc = GameComponents.GetPlayer().GetComponentInChildren<ScoreDisplayControl>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void StartTasks(PoolItem tpi, int spawnCount)
    {
        if (tpi.MainTaskPos != USTask.POSITION.None)
            Debug.Log("Main Task Spawned " + tpi.MainTaskPos + " " + spawnCount);
        else
            Debug.Log("Secondary Task Spawned " + tpi.SecondaryTaskPos + " " + spawnCount);

        USTask mainTask = null;

        if (tpi.MainTaskPos != USTask.POSITION.None)
        {
            mainTask = SpawnMainTask(tpi.MainTaskPos, spawnCount);
        }

        StartCoroutine(WaitForTaskActivation(tpi, spawnCount, mainTask));
    }

    private IEnumerator WaitForTaskActivation(PoolItem tpi, int spawnCount, USTask mainTask)
    {
        if (tpi.SecondaryTaskPos == USTask.POSITION.None)
        {
            yield return new WaitForSecondsRealtime(MainTaskActivationDelay);
            // mainTask.TryActivate();
            // OnTaskStarted(USTask.TYPE.Main, mainTask, tpi.MainTaskPos, spawnCount);
        }
        else if (_usc.GetCurrentFeedbackType() == UserStudyControl.FeedbackType.Audio)
        {
            StartCoroutine(StartAudioTask(tpi, spawnCount, mainTask));
        }
        else
        {
            StartCoroutine(StartSenseTask(tpi, spawnCount, mainTask));
        }
    }

    private IEnumerator StartAudioTask(PoolItem tpi, int spawnCount, USTask mainTask)
    {
        AudioClip voice = GetVoiceForSecondaryTask(tpi.SecondaryTaskPos);

        _audioSource.PlayOneShot(voice, _usc.TaskVoiceVolume);
        yield return new WaitForSecondsRealtime(MainTaskActivationDelay + AudioDelay);

        SpawnSecondaryTask(tpi.SecondaryTaskPos, spawnCount);
        ActivateTask(mainTask, tpi.MainTaskPos, spawnCount);
    }

    private IEnumerator StartSenseTask(PoolItem tpi, int spawnCount, USTask mainTask)
    {
        Debug.Log("Start Sense Task..");
        string feedbackData = _usc.GetFeedbackData(tpi.SecondaryTaskPos);
        if (feedbackData == null) Debug.LogError("No feedback data for main task pos");

        //float senseLatency = _fbs.GetLatencyForFeedbackType(_usc.GetCurrentFeedbackType());
        //yield return new WaitForSecondsRealtime(MainTaskActivationDelay + senseLatency);

        _feedbackUSB.UpdateFeedback(feedbackData, () =>
        {
            // callback waits till feedback is perceptible by player
            SpawnSecondaryTask(tpi.SecondaryTaskPos, spawnCount);
            // ActivateTask(mainTask, tpi.MainTaskPos, spawnCount);
        });

        yield break;
    }

    private void ActivateTask(USTask task, USTask.POSITION pos, int spawnCount)
    {
        if (task == null) return;
        task.TryActivate();
        OnTaskStarted(USTask.TYPE.Main, task, pos, spawnCount);
    }

    private USTask SpawnMainTask(USTask.POSITION pos, int spawnCount)
    {
        var mainTask = gameObject.AddComponent<USTask>();

        mainTask.StartNewTask(USTask.TYPE.Main, pos, (success) =>
        {
            OnTaskEnded(USTask.TYPE.Main, mainTask, pos, spawnCount, success ? "success" : "timeout");
            Debug.Log("Main Task " + (success ? "Success" : "Timeout") + " " + spawnCount);
            Destroy(mainTask);
        });

        OnTaskStarted(USTask.TYPE.Main, mainTask, pos, spawnCount);

        return mainTask;
    }

    private void SpawnSecondaryTask(USTask.POSITION pos, int spawnCount)
    {
        var secondaryTask = gameObject.AddComponent<USTask>();

        secondaryTask.StartNewTask(USTask.TYPE.Secondary, pos, (success) =>
        {
            HandleSecondaryTaskEnded(secondaryTask, pos, spawnCount, success);
            Destroy(secondaryTask);
        });

        OnTaskStarted(USTask.TYPE.Secondary, secondaryTask, pos, spawnCount);
    }

    private void HandleSecondaryTaskEnded(USTask task, USTask.POSITION pos, int spawnCount, bool success)
    {
        OnTaskEnded(USTask.TYPE.Secondary, task, pos, spawnCount, success ? "success" : "timeout");
        Debug.Log("Secondary Task " + (success ? "Success" : "Timeout") + " " + spawnCount);

        if (success)
        {
            _audioSource.PlayOneShot(_usc.SuccessSound, _usc.SuccessVolume);
            _scc.AddScore(1);
        }
        else
        {
            //_audioSource.PlayOneShot(_usc.TimeOutSound, _usc.TimeOutVolume);
            //_scc.AddScore(-2);
        }
    }

    private AudioClip GetVoiceForSecondaryTask(USTask.POSITION pos)
    {
        if (pos == USTask.POSITION.Left)
            return _usc.VoicePovLeft;

        if (pos == USTask.POSITION.Right)
            return _usc.VoicePovRight;

        return _usc.VoicePovMiddle;
    }

    protected virtual void OnTaskSpawned(USTask.TYPE t, USTask task, USTask.POSITION p, int c)
    {
        if (TaskSpawned != null)
            TaskSpawned(this, new USTaskControllerEventArgs { Type = t, Position = p, SpawnCount = c, Task = task });
    }

    protected virtual void OnTaskStarted(USTask.TYPE t, USTask task, USTask.POSITION p, int c)
    {
        if (TaskStarted != null)
            TaskStarted(this, new USTaskControllerEventArgs { Type = t, Position = p, SpawnCount = c, Task = task });
    }

    protected virtual void OnTaskEnded(USTask.TYPE t, USTask task, USTask.POSITION p, int c, string info)
    {
        if (TaskEnded != null)
            TaskEnded(this, new USTaskControllerEventArgs { Type = t, Position = p, SpawnCount = c, Task = task, EventInfo = info });
    }
}