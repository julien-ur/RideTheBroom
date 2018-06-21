using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UserStudyControl : MonoBehaviour {

    public enum FeedbackType { Heat, Smell, Vibration, Audio }
    private readonly string[] _feedbackLabels = { "Wärme", "Geruch", "Vibration", "Übung" };

    public static Dictionary<string, string> FEEDBACK_DICT = new Dictionary<string, string>
    {
        { "" + FeedbackType.Heat + USTask.POSITION.Left, FeedbackServer.HEAT_TAG + "0, 2" },
        { "" + FeedbackType.Heat + USTask.POSITION.Middle, FeedbackServer.HEAT_TAG + "1, 2" },
        { "" + FeedbackType.Heat + USTask.POSITION.Right, FeedbackServer.HEAT_TAG + "1, 0.5" },

        { "" + FeedbackType.Smell + USTask.POSITION.Left, FeedbackServer.SMELL_TAG + FeedbackServer.SMELL_WOODY_VAL + ", 2" },
        { "" + FeedbackType.Smell + USTask.POSITION.Middle, FeedbackServer.SMELL_TAG + FeedbackServer.SMELL_LEMON_VAL + ", 2" },
        { "" + FeedbackType.Smell + USTask.POSITION.Right, FeedbackServer.SMELL_TAG + FeedbackServer.SMELL_BERRY_VAL + ", 2" },

        { "" + FeedbackType.Vibration + USTask.POSITION.Left, FeedbackServer.VIBRATION_TAG + "1, 1.5;" },
        { "" + FeedbackType.Vibration + USTask.POSITION.Middle, FeedbackServer.VIBRATION_TAG + "1, 0.4;" },
        { "" + FeedbackType.Vibration + USTask.POSITION.Right, FeedbackServer.VIBRATION_TAG + "0.4, 2;" }
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

    private USTaskSpawner _spawner;
    private Text _infoText;
    private LoadingOverlay _loadingOverlay;
     
    private int _subjectId;
    private List<FeedbackType> _rounds;
    private FeedbackType _currentFeedbackType;
    private bool _playerReady;
    private bool _roundFinished;
    private USLogging _logging;
    private PlayerControl _pc;

    void Awake()
    {
        var u = new GameObject() { name = "User Study" };

        u.AddComponent<AudioSource>();
        _spawner = u.AddComponent<USTaskSpawner>();
        var taskControl = u.AddComponent<USTaskController>();
        
        _logging = u.AddComponent<USLogging>();

        _spawner.ActionCountReached += OnRoundFinished;
        taskControl.TaskStarted += OnTaskStarted;
        taskControl.TaskEnded += OnTaskEnded;

        _subjectId = Directory.GetFiles(UserStudyPath, "*.csv").Length;
        _rounds = new List<FeedbackType> { FeedbackType.Audio, FeedbackType.Audio };

        AddRoundsFromRoundConfig();
    }


    void Start()
    {
        _pc = GameComponents.GetPlayerControl();
        _loadingOverlay = GameComponents.GetLoadingOverlay();
        _infoText = GameComponents.GetVrHUD().transform.Find("StudyInfoText").GetComponentInChildren<Text>();

        MenuCabinTrigger mct = GameComponents.GetMenuCabinTrigger();
        if (!mct) _playerReady = true;
        else mct.PlayerLeftTheBuilding += OnPlayerLeftTheBuilding;

        StartCoroutine(StartStudy());
    }

    public void OnPlayerLeftTheBuilding(object sender, EventArgs args)
    {
        _playerReady = true;
        GameComponents.GetMenuObject().SetActive(false);
    }

    IEnumerator StartStudy()
    {
        yield return new WaitUntil(() => _playerReady);
        yield return new WaitForSecondsRealtime(2f);
        
        Debug.Log("Study Started - Subject #" + _subjectId);
        _logging.StartLogging("Subject #" + _subjectId);

        var roundCount = 0;

        foreach (FeedbackType f in _rounds)
        {
            _roundFinished = false;
            _currentFeedbackType = f;
            _spawner.InitNewRound(++roundCount == -1);

            //_loadingOverlay.FadeOut(1);
            //_pc.ChangeSpeedToTargetSpeed(0, 1);
            //yield return new WaitForSecondsRealtime(1.5f);
            //Time.timeScale = 0;

            //// pause game for questionaires
            //_infoText.text = (f != FeedbackType.Audio) ? "Time to answer some questions" : _feedbackLabels[(int)f];
            //yield return new WaitUntil(() => Input.GetKeyDown("space"));
            //_infoText.text = "";
            //yield return new WaitForSecondsRealtime(1.5f);

            //// show round label
            //if (f != FeedbackType.Audio)
            //{
            //    _infoText.text = _feedbackLabels[(int)f];
            //    yield return new WaitForSecondsRealtime(3);
            //    _infoText.text = "";
            //}

            //Time.timeScale = 1;

            //_loadingOverlay.FadeIn(2);
            //_pc.ChangeSpeedToDefaultSpeed(2);
            //yield return new WaitForSecondsRealtime(2);

            _spawner.StartSpawning();

            yield return new WaitUntil(() => _roundFinished);
        }

        Debug.Log("Study Finished");
        _logging.FinishLogging();

        _loadingOverlay.FadeOut(2);
        yield return new WaitForSecondsRealtime(2);
        _infoText.text = "Danke für deine Teilnahme!";
        _infoText.resizeTextForBestFit = true;

        yield return new WaitForSecondsRealtime(5);

        _infoText.text = "";
        _loadingOverlay.FadeIn(2);
        yield return new WaitForSecondsRealtime(2);
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
            _pc.LimitRotationScopeByAxis('x', 30);
            _pc.LimitRotationScopeByAxis('y', 65);
        }
    }

    private void OnTaskEnded(object sender, USTaskControllerEventArgs args)
    {
        if (args.Type == USTask.TYPE.Main)
        {
            _pc.BlockRotationForAxis("xy", true);
        }
    }

    public void OnRoundFinished(object sender, EventArgs args)
    {
        Debug.Log("Round Finished");
        _roundFinished = true;
    }

    public FeedbackType GetCurrentFeedbackType()
    {
        return _currentFeedbackType;
    }

    public string GetFeedbackData(USTask.POSITION actionPosition)
    {
        string feedbackData;
        FEEDBACK_DICT.TryGetValue("" + _currentFeedbackType + actionPosition, out feedbackData);

        if (feedbackData != null)
            feedbackData = feedbackData.Replace(" ", "");
        else
            Debug.LogError("No feedback data for " + _currentFeedbackType + " " + actionPosition);

        return feedbackData;
    }
}
