using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    public enum FeedbackType { Heat, Smell, Vibration, Audio }
    private readonly string[] _feedbackLabels = { "Wärme", "Geruch", "Vibration", "Audio" };

    public static Dictionary<string, string> FEEDBACK_DICT = new Dictionary<string, string>
    {
        { "" + FeedbackType.Heat + " " + USTask.POSITION.Left, FeedbackConstants.HEAT_TAG + ",0,3" },
        { "" + FeedbackType.Heat + " " + USTask.POSITION.Middle, FeedbackConstants.HEAT_TAG + ",0.5,3" },
        { "" + FeedbackType.Heat + " " + USTask.POSITION.Right, FeedbackConstants.HEAT_TAG + ",1,3" },

        { "" + FeedbackType.Smell + " " + USTask.POSITION.Left, FeedbackConstants.SMELL_TAG + "," + FeedbackConstants.SMELL_WOODY_VAL + ",0.8" },
        { "" + FeedbackType.Smell + " " + USTask.POSITION.Middle, FeedbackConstants.SMELL_TAG + "," + FeedbackConstants.SMELL_LEMON_VAL + ",1" },
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
    private int _roundCount;
    private ScoreDisplayControl _scc;


    void Start()
    {
        var u = new GameObject() { name = "User Study" };

        u.AddComponent<AudioSource>();
        _spawner = u.AddComponent<USTaskSpawner>();
        var taskControl = u.AddComponent<USTaskController>();

        _logging = u.AddComponent<USLogging>();

        _spawner.ActionCountReached += OnSpawnerFinished;
        taskControl.TaskSpawned += OnTaskStarted;
        taskControl.TaskEnded += OnTaskEnded;

        _subjectId = Directory.GetFiles(UserStudyPath, "*.csv").Length / 2;
        _rounds = new List<FeedbackType> { FeedbackType.Audio, FeedbackType.Audio };

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

        StartCoroutine(StartStudy());

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

    IEnumerator StartStudy()
    {
        yield return new WaitUntil(() => _playerReady);
        _pc.BlockRotationForAxis("x");
        
        yield return new WaitForSecondsRealtime(2f);

        Debug.Log("Study Started - Subject #" + _subjectId);
        OnStudyStarted();

        _roundCount = 0;

        foreach (FeedbackType f in _rounds)
        {
            _roundFinished = false;
            HasFeedbackPresented = false;
            _currentFeedbackType = f;

            // PAUSE GAME AND SHOW INFO TEXT //
            _pc.ChangeSpeedToTargetSpeed(0, 1);
            // _loadingOverlay.FadeOut(2);
            yield return new WaitForSecondsRealtime(1.5f);
            Time.timeScale = 0;

            if (_roundCount > 0)
            {
                _infoText.text = "Pause";
                yield return new WaitUntil(() => Input.GetKeyUp("space"));
                yield return new WaitForEndOfFrame();
                Time.timeScale = 1;
                if (_roundCount == 1) _scc.SetScore(0);
            }

            _infoText.text = (_roundCount > 0) ? _feedbackLabels[(int)f] : "Übung";
            if (f != FeedbackType.Audio)
            {
                yield return new WaitUntil(() => HasFeedbackPresented);
            }
            yield return new WaitUntil(() => Input.GetKeyUp("space"));

            _infoText.text = "";
            Time.timeScale = 1;

            // _loadingOverlay.FadeIn(2);
            _pc.ChangeSpeedToDefaultSpeed(2);
            yield return new WaitForSecondsRealtime(2);
            // ----------------------------------------------------------------- //

            OnRoundStarted();
            _spawner.InitNewRound(_roundCount == 0);

            _spawner.StartSpawning();

            yield return new WaitUntil(() => _roundFinished);
            OnRoundFinished();
            _roundCount++;
        }

        Debug.Log("Study Finished");
        OnStudyFinished();

        // _loadingOverlay.FadeOut(2);
        _pc.ChangeSpeedToTargetSpeed(0, 1);
        yield return new WaitForSecondsRealtime(1.5f);
        Time.timeScale = 0;
        _infoText.text = "Ende";

        yield return new WaitForSecondsRealtime(3);
        _feedbackUSB.StopAllFeedback();
        GameComponents.GetGameController().FinishLevel();
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

    public void OnSpawnerFinished(object sender, EventArgs args)
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
            RoundStarted(this, new UserStudyEventArgs() { SubjectID = _subjectId, FeedbackType = GetCurrentFeedbackType() });
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

    public FeedbackType GetCurrentFeedbackType()
    {
        return _currentFeedbackType;
    }

    public int GetCurrentRoundCount()
    {
        return _roundCount;
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

    public object GetSubjectId()
    {
        return _subjectId;
    }
}
