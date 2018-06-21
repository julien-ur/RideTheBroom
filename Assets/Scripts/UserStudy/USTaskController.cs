using System;
using System.Collections;
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
    private FeedbackServer _fsr;
    private ScoreDisplayControl _scc;
    private AudioSource _audioSource;

    private float MainTaskActivationDelay = 2f;

    private AudioClip _timeOutSound;
    private AudioClip _successSound;
    private float _timeOutVolume;
    private float _successVolume;

    void Awake()
    {
        _usc = GameComponents.GetLevelControl().GetComponent<UserStudyControl>();
        _fsr = GameComponents.GetGameController().GetComponent<FeedbackServer>();
        _scc = GameComponents.GetPlayer().GetComponentInChildren<ScoreDisplayControl>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void StartTasks(PoolItem tpi, int spawnCount)
    {
        Debug.Log("Tasks Spawned " + tpi.MainTaskPos + " " + tpi.SecondaryTaskPos + " " + spawnCount);

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
            yield return new WaitForSecondsRealtime(3f);
            mainTask.Activate();
            OnTaskStarted(USTask.TYPE.Main, tpi.MainTaskPos, spawnCount);
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
        yield return new WaitForSecondsRealtime(MainTaskActivationDelay + voice.length - 0.5f);

        _audioSource.PlayOneShot(voice, _usc.TaskVoiceVolume);
        SpawnSecondaryTask(tpi.SecondaryTaskPos, spawnCount);
        if (mainTask == null) yield break;
        mainTask.Activate();
        OnTaskStarted(USTask.TYPE.Main, tpi.MainTaskPos, spawnCount);
    }

    private IEnumerator StartSenseTask(PoolItem tpi, int spawnCount, USTask mainTask)
    {
        string feedbackData = _usc.GetFeedbackData(tpi.MainTaskPos);
        if (feedbackData == null) Debug.LogError("No feedback data for main task pos");

        float senseLatency = _fsr.GetLatencyForFeedbackType(_usc.GetCurrentFeedbackType());
        yield return new WaitForSecondsRealtime(MainTaskActivationDelay + senseLatency);

        _fsr.PostChange(feedbackData, () =>
        {
            // callback waits till feedback is perceptible by player
            SpawnSecondaryTask(tpi.SecondaryTaskPos, spawnCount);
            if (mainTask == null) return;
            mainTask.Activate();
            OnTaskStarted(USTask.TYPE.Main, tpi.MainTaskPos, spawnCount);
        });
    }

    private USTask SpawnMainTask(USTask.POSITION pos, int spawnCount)
    {
        var mainTask = gameObject.AddComponent<USTask>();

        mainTask.StartNewTask(USTask.TYPE.Main, pos, (success) =>
        {
            OnTaskEnded(USTask.TYPE.Main, pos, spawnCount, success ? "Success" : "Timeout");
            Debug.Log("Main Task " + (success ? "Success" : "Timeout") + " " + spawnCount);
            Destroy(mainTask);
        });

        OnTaskSpawned(USTask.TYPE.Main, mainTask, pos, spawnCount);

        return mainTask;
    }

    private void SpawnSecondaryTask(USTask.POSITION pos, int spawnCount)
    {
        var secondaryTask = gameObject.AddComponent<USTask>();

        secondaryTask.StartNewTask(USTask.TYPE.Secondary, pos, (success) =>
        {
            HandleSecondaryTaskEnded(pos, spawnCount, success);
            Destroy(secondaryTask);
        });

        OnTaskStarted(USTask.TYPE.Secondary, pos, spawnCount);
    }

    private void HandleSecondaryTaskEnded(USTask.POSITION pos, int spawnCount, bool success)
    {
        OnTaskEnded(USTask.TYPE.Secondary, pos, spawnCount, success ? "Success" : "Timeout");
        Debug.Log("Secondary Task " + (success ? "Success" : "Timeout") + " " + spawnCount);

        if (success)
        {
            _audioSource.PlayOneShot(_usc.SuccessSound, _usc.SuccessVolume);
            _scc.AddScore(5);
        }
        else
        {
            _audioSource.PlayOneShot(_usc.TimeOutSound, _usc.TimeOutVolume);
            _scc.AddScore(-2);
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

    protected virtual void OnTaskStarted(USTask.TYPE t, USTask.POSITION p, int c)
    {
        if (TaskStarted != null)
            TaskStarted(this, new USTaskControllerEventArgs { Type = t, Position = p, SpawnCount = c });
    }

    protected virtual void OnTaskEnded(USTask.TYPE t, USTask.POSITION p, int c, string info)
    {
        if (TaskEnded != null)
            TaskEnded(this, new USTaskControllerEventArgs { Type = t, Position = p, SpawnCount = c, EventInfo = info });
    }
}