using System;
using UnityEngine;

public class USTaskControllerEventArgs : EventArgs
{
    public USTask.TYPE Type;
    public USTask.POSITION Position;
    public int SpawnCount;
    public string EventInfo;
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

    public void StartTasks(PoolItem item, int spawnCount)
    {
        Debug.Log("Tasks Spawned " + item.MainTaskPos + " " + item.SecondaryTaskPos + " " + spawnCount);
        
        if (_usc.GetCurrentFeedbackType() == UserStudyControl.FeedbackType.Audio)
        {
            if (item.MainTaskPos != USTask.POSITION.None)
            {
                SpawnMainTask(item.MainTaskPos, spawnCount);
            }
            if (item.SecondaryTaskPos != USTask.POSITION.None)
            {
                _audioSource.PlayOneShot(GetVoiceForSecondaryTask(item.SecondaryTaskPos), _usc.TaskVoiceVolume);
                SpawnSecondaryTask(item.SecondaryTaskPos, spawnCount);
            }
        }
        else
        {
            if (item.MainTaskPos != USTask.POSITION.None && item.SecondaryTaskPos == USTask.POSITION.None)
            {
                SpawnMainTask(item.MainTaskPos, spawnCount);
            }
            else if (item.SecondaryTaskPos != USTask.POSITION.None)
            {
                string feedbackData = _usc.GetFeedbackData(item.MainTaskPos);
                if (feedbackData == null) return;
                _fsr.PostChange(feedbackData, () =>
                {
                    SpawnSecondaryTask(item.SecondaryTaskPos, spawnCount);

                    if (item.MainTaskPos != USTask.POSITION.None)
                        SpawnMainTask(item.MainTaskPos, spawnCount);
                });
            }
        }
    }

    private void SpawnMainTask(USTask.POSITION pos, int spawnCount)
    {
        var mainTask = gameObject.AddComponent<USTask>();

        mainTask.StartNewAction(USTask.TYPE.Main, pos, (success) =>
        {
            OnTaskEnded(USTask.TYPE.Main, pos, spawnCount, success ? "Success" : "Timeout");
            Debug.Log("Main Task " + (success ? "Success" : "Timeout") + " " + spawnCount);
            Destroy(mainTask);
        });

        OnTaskStarted(USTask.TYPE.Main, pos, spawnCount);
    }

    private void SpawnSecondaryTask(USTask.POSITION pos, int spawnCount)
    {
        var secondaryTask = gameObject.AddComponent<USTask>();

        secondaryTask.StartNewAction(USTask.TYPE.Secondary, pos, (success) =>
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