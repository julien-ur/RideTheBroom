using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class UserStudyEventArgs :  EventArgs
{
    public int SubjectID;
    public UserStudyControl.FeedbackType FeedbackType;
}

public class UserStudyControl : MonoBehaviour
{
    public static EventHandler<UserStudyEventArgs> StudyStarted;
    public static EventHandler<UserStudyEventArgs> RoundStarted;
    public static EventHandler RoundFinished;
    public static EventHandler StudyFinished;

    public enum FeedbackType { Heat, Smell, Vibration, Audio, Training_Ring_Only, Training_Complete }
    private readonly string[] _feedbackLabels = { "Wärme", "Geruch", "Vibration", "Audio", "Training I", "Training II" };

    public static Dictionary<string, string> FEEDBACK_DICT = new Dictionary<string, string>
    {
        { "" + FeedbackType.Heat + " " + USTask.POSITION.Left, FeedbackConstants.HEAT_TAG + ",0,3" },
        { "" + FeedbackType.Heat + " " + USTask.POSITION.Middle, FeedbackConstants.HEAT_TAG + ",0.5,3" },
        { "" + FeedbackType.Heat + " " + USTask.POSITION.Right, FeedbackConstants.HEAT_TAG + ",1,3" },

        { "" + FeedbackType.Smell + " " + USTask.POSITION.Left, FeedbackConstants.SMELL_TAG + "," + FeedbackConstants.SMELL_WOODY_VAL + ",0.5" },
        { "" + FeedbackType.Smell + " " + USTask.POSITION.Middle, FeedbackConstants.SMELL_TAG + "," + FeedbackConstants.SMELL_LEMON_VAL + ",0.8" },
        { "" + FeedbackType.Smell + " " + USTask.POSITION.Right, FeedbackConstants.SMELL_TAG + "," + FeedbackConstants.SMELL_BERRY_VAL + ",0.8" },

        { "" + FeedbackType.Vibration + " " + USTask.POSITION.Left, FeedbackConstants.VIBRATION_TAG + ",0.2,0.5" },
        { "" + FeedbackType.Vibration + " " + USTask.POSITION.Middle, FeedbackConstants.VIBRATION_TAG + ",0.35,0.5" },
        { "" + FeedbackType.Vibration + " " + USTask.POSITION.Right, FeedbackConstants.VIBRATION_TAG + ",0.7,0.5" }
    };

    public static Dictionary<string, float> DEFAULT_FEEDBACK = new Dictionary<string, float>
    {
        { FeedbackConstants.WIND_TAG, 0.4f },
        { FeedbackConstants.HEAT_TAG, 0.2f },
    };

    public AudioClip TimeOutSound;
    public float TimeOutVolume = 0.3f;

    public AudioClip SuccessSound;
    public float SuccessVolume = 0.3f;

    public AudioClip PovSelectingSound;
    public float PovSelectingVolume = 0.3f;

    public AudioClip VoicePovLeft;
    public AudioClip VoicePovMiddle;
    public AudioClip VoicePovRight;
    public float TaskVoiceVolume = 1f;

    public GameObject RefillTankItem;
    public GameObject PovContainer;
    public GameObject RingObject;
    public GameObject RingInactiveObject;

    public static string UserStudyPath = "UserStudy";
    public static string RoundConfigName = "round_config.ini";
    public static string RoundConfigPath = UserStudyPath + "/" + RoundConfigName;

    public static bool HasFeedbackPresented;
    public static bool UserMadeInput;
    public static bool _repeatRound;

    private USTaskSpawner _spawner;
    private Text _infoText;
    private LoadingOverlay _loadingOverlay;
     
    private int _subjectId;
    private List<FeedbackType> _rounds;
    private FeedbackType _currentFeedbackType;
    //private FeedbackServer _fbs;
    private FeedbackUSB _feedbackUSB;
    private bool _playerReady;
    private bool _roundFinished;
    private USLogging _logging;
    private PlayerControl _pc;
    private int _roundNum;
    private ScoreDisplayControl _scc;
    private bool _trainingFinished;
    private bool _studyPaused;
    private int _trainingPhase;
    private int _roundRepeatCount;
    private bool _showLabel;


    void Start()
    {
        var u = new GameObject() { name = "User Study" };

        u.AddComponent<AudioSource>();
        _spawner = u.AddComponent<USTaskSpawner>();
        var taskControl = u.AddComponent<USTaskController>();

        _logging = u.AddComponent<USLogging>();

        _spawner.ActionCountReached += OnRoundFinished;
        taskControl.TaskSpawned += OnTaskStarted;
        taskControl.TaskEnded += OnTaskEnded;

        _rounds = new List<FeedbackType> { FeedbackType.Training_Ring_Only, FeedbackType.Training_Complete, FeedbackType.Audio };

        AddRoundsFromRoundConfig();

        _pc = GameComponents.GetPlayerControl();
        //_fbs = GameComponents.GetGameController().GetComponent<FeedbackServer>();
        _feedbackUSB = GameComponents.GetGameController().GetComponent<FeedbackUSB>();
        _loadingOverlay = GameComponents.GetLoadingOverlay();
        _infoText = GameComponents.GetVrHUD().transform.Find("StudyInfoText").GetComponentInChildren<Text>();
        _scc = GameComponents.GetPlayer().GetComponentInChildren<ScoreDisplayControl>();

        MenuCabinTrigger mct = GameComponents.GetMenuCabinTrigger();
        if (!mct) {
            _playerReady = true;
        }
        else
        {
            mct.PlayerLeftTheBuilding += OnPlayerLeftTheBuilding;
        }

        _pc.LimitRotationScopeByAxis('x', 30);

        StartCoroutine(StudyLoop());

        foreach (var entry in DEFAULT_FEEDBACK)
        {
            _feedbackUSB.PermanentUpdate(entry.Key, entry.Value);
        }
    }

    public void OnPlayerLeftTheBuilding(object sender, EventArgs args)
    {
        _playerReady = true;
        GameComponents.GetMenuObject().SetActive(false);
    }

    private IEnumerator StudyLoop()
    {
        yield return new WaitUntil(() => _playerReady);
        _pc.BlockRotationForAxis("x");
        
        yield return new WaitForSecondsRealtime(2f);

        Debug.Log("Study Started - S" + _subjectId);
        OnStudyStarted();

        _roundNum = 0;
        
        StartCoroutine(PauseGame());
        yield return new WaitUntil(() => _studyPaused);

        while (_roundNum < _rounds.Count)
        {
            _roundFinished = false;
            _currentFeedbackType = _rounds[_roundNum];
            
            // reset score after training
            if (_roundNum == 2) _scc.SetScore(0);
            
            StartCoroutine(ShowRoundTypeLabel());
            yield return new WaitUntil(() => _infoText.text == "");
            
            StartCoroutine(ResumeGame());
            yield return new WaitUntil(() => !_studyPaused);
            
            var spawnPoolType = _currentFeedbackType == FeedbackType.Training_Ring_Only
                ? USTaskPoolGenerator.MODE.TRAINING_RING_ONLY
                : USTaskPoolGenerator.MODE.STUDY;
            
            _spawner.InitNewRound(spawnPoolType);
            
            OnRoundStarted();
            _spawner.StartSpawning();
            yield return new WaitUntil(() => _roundFinished);
            OnRoundFinished();

            StartCoroutine(PauseGame());
            yield return new WaitUntil(() => _studyPaused);

            StartCoroutine(ShowPauseLabel());
            yield return new WaitUntil(() => _infoText.text == "");

            CheckRepeatAndUpdateCounts();
        }

        Debug.Log("Study Finished");
        OnStudyFinished();

        StartCoroutine(PauseGame());
        yield return new WaitUntil(() => _studyPaused);
        _infoText.text = "Ende";

        yield return new WaitForSecondsRealtime(3);
        _feedbackUSB.StopAllFeedback();
        GameComponents.GetGameController().FinishLevel();
    }

    private IEnumerator PauseGame()
    {
        // freeze speed & time
        _pc.ChangeSpeedToTargetSpeed(0, 1);
        yield return new WaitForSecondsRealtime(1.5f);
        Time.timeScale = 0;
        _studyPaused = true;
    }

    private IEnumerator ResumeGame()
    {
        // unfreeze speed & time
        Time.timeScale = 1;
        _pc.ChangeSpeedToDefaultSpeed(2);
        yield return new WaitForSecondsRealtime(2);
        _studyPaused = false;
    }
    
    private IEnumerator ShowPauseLabel()
    {
        // show pause label
        _infoText.text = "Pause";
        yield return new WaitUntil(() => Input.GetKeyUp("space"));
        yield return new WaitForEndOfFrame();
        _infoText.text = "";
    }

    private IEnumerator ShowRoundTypeLabel()
    {
        // show round info
        _infoText.text = GetRoundLabel();
        
        // wait for feedback presentation if needed
        if (GetFeedbackType() != FeedbackType.Audio)
        {
            HasFeedbackPresented = false;
            yield return new WaitUntil(() => HasFeedbackPresented);
        }
        yield return new WaitUntil(() => Input.GetKeyUp("space"));

        _infoText.text = "";
    }
    
    private void CheckRepeatAndUpdateCounts()
    {
        if (_repeatRound)
        {
            _roundRepeatCount ++;
            _repeatRound = false;
        }
        else
        {
            _roundNum ++;
            _roundRepeatCount = 0;
        }
    }

    private void AddRoundsFromRoundConfig()
    {
        var lines = File.ReadAllLines(RoundConfigPath);
        if (lines.Length != 6)
            Debug.LogError("Not the right amount of entries (6) in round config");

        try
        {
            string line = lines[_subjectId % 6];
            var roundOrder = line.Split(',');

            var fll = _feedbackLabels.ToList();

            foreach (string typeLabel in roundOrder)
            {
                _rounds.Add((FeedbackType)fll.IndexOf(typeLabel));
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void OnTaskStarted(object sender, USTaskControllerEventArgs args)
    {
        if (args.Type == USTask.TYPE.Main)
        {
            //_pc.LimitRotationScopeByAxis('x', 30);
            //_pc.LimitRotationScopeByAxis('y', 65);
        }
    }

    private void OnTaskEnded(object sender, USTaskControllerEventArgs args)
    {
        if (args.Type == USTask.TYPE.Main)
        {
            //_pc.BlockRotationForAxis("xy", true);
        }
    }

    public void OnRoundFinished(object sender, EventArgs args)
    {
        Debug.Log("Round Finished");
        _roundFinished = true;
    }

    protected virtual void OnStudyStarted()
    {
        if (StudyStarted != null)
            StudyStarted(this, new UserStudyEventArgs() { SubjectID = _subjectId, FeedbackType = _rounds[0] });
    }

    protected virtual void OnRoundStarted()
    {
        if (RoundStarted != null)
            RoundStarted(this, new UserStudyEventArgs() { SubjectID = _subjectId, FeedbackType = GetFeedbackType() });
    }

    protected virtual void OnRoundFinished()
    {
        if (RoundFinished != null)
            RoundFinished(this, EventArgs.Empty);
    }

    protected virtual void OnStudyFinished()
    {
        if (StudyFinished != null)
            StudyFinished(this, EventArgs.Empty);
    }
    
    public static void ToggleRoundRepeat()
    {
        _repeatRound = !_repeatRound;
    }

    public void ForceFinishRound()
    {
        Debug.Log("Round manually finished..");
        _roundFinished = true;
        _spawner.StopSpawning();
    }

    public FeedbackType GetFeedbackType()
    {
        return (int)_currentFeedbackType > 2 ? FeedbackType.Audio : _currentFeedbackType;
    }
    
    public object GetRoundType()
    {
        return _currentFeedbackType;
    }
    
    private string GetRoundLabel()
    {
        return _feedbackLabels[(int) _currentFeedbackType];
    }

    public int GetCurrentRoundCount()
    {
        return _roundNum;
    }
    
    public int GetCurrentRepeatCount()
    {
        return _roundRepeatCount;
    }

    public static bool IsRoundRepeated()
    {
        return _repeatRound;
    }
    
    public string GetFeedbackData(USTask.POSITION actionPosition)
    {
        string feedbackData;
        FEEDBACK_DICT.TryGetValue("" + _currentFeedbackType + " " + actionPosition, out feedbackData);

        if (feedbackData != null)
            feedbackData = feedbackData.Replace(" ", "") + ";";
        else
            Debug.LogError("No feedback data for " + _currentFeedbackType + " " + actionPosition);

        return feedbackData;
    }

    public int GetSubjectId()
    {
        return _subjectId;
    }
    
    public int GetAutoSubjectId()
    {
        return Directory.GetFiles(UserStudyPath, "*.csv").Length / 2;
    }
    
    public void SetSubjectId(int s)
    {
        _subjectId = s;
    }
}
