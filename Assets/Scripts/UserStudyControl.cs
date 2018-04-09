using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UserStudyControlEventArgs : EventArgs
{
    public string EventInfo { get; set; }
}

public class UserStudyControl : MonoBehaviour {

    public enum FeedbackType { Heat, Smell, Vibration }
    public string[] FeedbackLabels = { "Wärme", "Geruch", "Vibration" };

    public static Dictionary<string, string> FEEDBACK_DICT = new Dictionary<string, string>
    {
        { "" + FeedbackType.Heat + USAction.TYPE.RefillTank, FeedbackServer.HEAT_TAG + "0, 2" },
        { "" + FeedbackType.Heat + USAction.TYPE.Accelerate, FeedbackServer.HEAT_TAG + "1, 2" },
        { "" + FeedbackType.Heat + USAction.TYPE.POV, FeedbackServer.HEAT_TAG + "1, 0.5" },

        { "" + FeedbackType.Smell + USAction.TYPE.RefillTank, FeedbackServer.SMELL_TAG + FeedbackServer.SMELL_WOODY_VAL + ", 2" },
        { "" + FeedbackType.Smell + USAction.TYPE.Accelerate, FeedbackServer.SMELL_TAG + FeedbackServer.SMELL_LEMON_VAL + ", 2" },
        { "" + FeedbackType.Smell + USAction.TYPE.POV, FeedbackServer.SMELL_TAG + FeedbackServer.SMELL_BERRY_VAL + ", 2" },

        { "" + FeedbackType.Vibration + USAction.TYPE.RefillTank, FeedbackServer.VIBRATION_TAG + "0.4, 2;" },
        { "" + FeedbackType.Vibration + USAction.TYPE.Accelerate, FeedbackServer.VIBRATION_TAG + "1, 2;" },
        { "" + FeedbackType.Vibration + USAction.TYPE.POV, FeedbackServer.VIBRATION_TAG + "0.4, 2;" }
    };

    public AudioClip TimeOutSound;
    public float TimeOutVolume = 0.3f;

    public AudioClip SuccessSound;
    public float SuccessVolume = 0.3f;

    public AudioClip PovSelectingSound;
    public float PovSelectingVolume = 0.3f;

    public GameObject RefillTankItem;
    public GameObject PovContainer;

    public static string UserStudyPath = "UserStudy";
    public static string RoundConfigName = "round_config.ini";
    public static string RoundConfigPath = UserStudyPath + "/" + RoundConfigName;

    private USActionSpawner _spawner;
    private USActionController _actionControl;
    private Text _infoText;
    private Fading _fading;
     
    private int _subjectId;
    private List<FeedbackType> _rounds;
    private FeedbackType _currentFeedbackType;
    private bool _playerReady;
    private bool _roundFinished;
    private USLogging _logging;

    void Awake()
    {
        InitStudyObjects();

        _subjectId = Directory.GetFiles(UserStudyPath, "*.csv").Length;
        _rounds = new List<FeedbackType>();

        if (File.Exists(RoundConfigPath) && TryGetRoundConfigEntry()) return;

        CreateNewRoundConfig();
        TryGetRoundConfigEntry();
    }

    private void CreateNewRoundConfig()
    {
        FeedbackType[] feedbackTypes = Enum.GetValues(typeof(FeedbackType)).Cast<FeedbackType>().ToArray();
          
        using (StreamWriter sw = File.AppendText(RoundConfigPath))
        {
            foreach (FeedbackType[] feedbackOrder in Utilities.Permutations(feedbackTypes))
            {
                if (_subjectId == 0)
                    _rounds = feedbackOrder.ToList();

                var line = "";
                foreach (FeedbackType t in feedbackOrder)
                {
                    line += FeedbackLabels[(int) t] + ",";
                }

                sw.WriteLine(line.TrimEnd(','));
            }
        }
    }

    private bool TryGetRoundConfigEntry()
    {
        var lines = File.ReadAllLines(RoundConfigPath);
        if (lines.Length != 6) return false;

        try
        {
            string line = lines[_subjectId % 6];
            var roundOrder = line.Split(',');

            var fll = FeedbackLabels.ToList();

            foreach (string typeLabel in roundOrder)
            {
                _rounds.Add((FeedbackType) fll.IndexOf(typeLabel));
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }

        return true;
    }

    void Start()
    {
        _fading = GameComponents.GetFading();

        MenuCabinTrigger mct = GameComponents.GetMenuCabinTrigger();
        if (!mct) _playerReady = true;
        else mct.PlayerLeftTheBuilding += OnPlayerLeftTheBuilding;

        StartCoroutine(StartStudy());
    }

    public void OnPlayerLeftTheBuilding(object sender, EventArgs args)
    {
        _playerReady = true;
    }

    IEnumerator StartStudy()
    {
        yield return new WaitUntil(() => _playerReady);
        yield return new WaitForSecondsRealtime(2f);
        
        Debug.Log("Study Started");
        _logging.StartLogging("Subject #" + _subjectId);

        foreach (FeedbackType f in _rounds)
        {
            _roundFinished = false;
            _currentFeedbackType = f;
            _spawner.ResetActionCount();

            //_fading.FadeOut(1);
            //_pc.changeSpeedToTargetSpeed(0, 1);
            //yield return new WaitForSecondsRealtime(1.5f);

            //_infoText.text = FeedbackLabels[(int)f];
            //yield return new WaitForSecondsRealtime(3);

            //_infoText.text = "";
            //_fading.FadeIn(2);
            //_pc.changeSpeedToDefaultSpeed(2);
            //yield return new WaitForSecondsRealtime(2);

            _spawner.StartSpawning();
            
            yield return new WaitUntil(() => _roundFinished);
        }

        Debug.Log("Study Finished");
        _logging.FinishLogging();

        _fading.FadeOut(2);
        yield return new WaitForSecondsRealtime(2);
        _infoText.text = "Danke für deine Teilnahme!";
        _infoText.resizeTextForBestFit = true;

        yield return new WaitForSecondsRealtime(5);

        _infoText.text = "";
        _fading.FadeIn(2);
        yield return new WaitForSecondsRealtime(2);
    }

    private void InitStudyObjects()
    {
        var u = new GameObject(){ name = "User Study" };

        u.AddComponent<AudioSource>();
        _spawner = u.AddComponent<USActionSpawner>();
        _actionControl = u.AddComponent<USActionController>();
        var action = u.AddComponent<USAction>();
        var prd = u.AddComponent<USPlayerReactionDetection>();
        _logging = u.AddComponent<USLogging>();

        _actionControl.SetTimeOutSound(TimeOutSound, TimeOutVolume);
        _actionControl.SetSuccessSound(SuccessSound, SuccessVolume);

        _infoText = GameObject.FindGameObjectWithTag("HUD").transform.Find("StudyInfoText").GetComponentInChildren<Text>();
        _infoText.resizeTextForBestFit = true;

        RegisterListener(action, prd);
    }

    private void RegisterListener(USAction action, USPlayerReactionDetection prd)
    {
        var fsr = GameComponents.GetGameController().GetComponent<FeedbackServer>();

        fsr.FeedbackRequestSent += _logging.OnRelevantStudyEvent;

        action.ActionStarted += _actionControl.OnActionStarted;
        action.ActionStarted += prd.OnActionStarted;
        action.ActionStarted += _logging.OnRelevantStudyEvent;

        action.ActionSuccess += _spawner.OnActionFinished;
        action.ActionSuccess += _actionControl.OnActionSuccess;
        action.ActionSuccess += prd.OnActionFinished;
        action.ActionSuccess += _logging.OnRelevantStudyEvent;

        action.ActionTimeOut += _spawner.OnActionFinished;
        action.ActionTimeOut += _actionControl.OnActionTimeOut;
        action.ActionTimeOut += prd.OnActionFinished;
        action.ActionTimeOut += _logging.OnRelevantStudyEvent;

        prd.ReactionDetectedUnsave += _logging.OnRelevantStudyEvent;
        prd.ReactionDetectedSave += _logging.OnRelevantStudyEvent;

        _spawner.ActionCountReached += OnRoundFinished;
    }

    public void OnRoundFinished(object sender, EventArgs args)
    {
        Debug.Log("Round Finished");
        _roundFinished = true;
    }

    public GameObject GetRefillTankItem()
    {
        return RefillTankItem;
    }

    public GameObject GetPovContainer()
    {
        return PovContainer;
    }

    public FeedbackType GetCurrentFeedbackType()
    {
        return _currentFeedbackType;
    }

    public string GetFeedbackData(USAction.TYPE actionType)
    {
        string feedbackData;
        FEEDBACK_DICT.TryGetValue("" + _currentFeedbackType + actionType, out feedbackData);

        if (feedbackData != null)
            feedbackData = feedbackData.Replace(" ", "");
        else
            Debug.LogError("No feedback data for " + _currentFeedbackType + " " + actionType);

        return feedbackData;
    }
}
