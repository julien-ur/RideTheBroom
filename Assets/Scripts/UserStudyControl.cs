using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserStudyControl : MonoBehaviour {

    public enum FeedbackType { Heat, Smell, Vibration }
    public string[] FeedbackLabels = { "Wärme", "Geruch", "Vibration" };
    public FeedbackType[] Rounds = { FeedbackType.Smell, FeedbackType.Vibration, FeedbackType.Heat };

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
        { "" + FeedbackType.Vibration + USAction.TYPE.POV, FeedbackServer.VIBRATION_TAG + "0.4, 0.3;" +
                                                         FeedbackServer.PAUSE_TAG + "0.5;" +
                                                         FeedbackServer.VIBRATION_TAG + "0.4, 0.3;" +
                                                         FeedbackServer.PAUSE_TAG + " 0.5;" +
                                                         FeedbackServer.VIBRATION_TAG + "0.4, 0.3" }
    };

    public AudioClip TimeOutSound;
    public float TimeOutVolume = 0.3f;

    public AudioClip SuccessSound;
    public float SuccessVolume = 0.3f;

    private USActionSpawner _spawner;
    private USActionController _actionControl;
    private Text _infoText;
    private Fading _fading;
    private MenuCabinTrigger _mct;

    private FeedbackType _currentFeedbackType;
    private bool _roundFinished;

    private void Start()
    {
        _fading = GameComponents.GetFading();
        _mct = GameComponents.GetMenuCabinTrigger();

        CreateStudyObjects();
        RegisterListener();
    }

    public void OnPlayerLeftTheBuilding(object sender, EventArgs args)
    {
        StartCoroutine(StartStudy());
    }

    IEnumerator StartStudy()
    {
        Debug.Log("Study Started");

        foreach (FeedbackType f in Rounds)
        {
            _roundFinished = false;
            _currentFeedbackType = f;
            _spawner.ResetActionCount();

            _fading.FadeOut(1, true);
            yield return new WaitForSecondsRealtime(1.5f);

            _infoText.text = FeedbackLabels[(int)f];
            yield return new WaitForSecondsRealtime(3);

            _infoText.text = "";
            _fading.FadeIn(2);
            yield return new WaitForSecondsRealtime(2);

            _spawner.StartSpawning();
            StartLogging();

            yield return new WaitUntil(() => _roundFinished);
        }

        Debug.Log("Study Finished");
        FinishLogging();

        _fading.FadeOut(2, true);
        yield return new WaitForSecondsRealtime(2);
        _infoText.text = "Danke für deine Teilnahme!";
        _infoText.resizeTextForBestFit = true;

        yield return new WaitForSecondsRealtime(5);

        _infoText.text = "";
        _fading.FadeIn(2);
        yield return new WaitForSecondsRealtime(2);
    }

    private void CreateStudyObjects()
    {
        var u = new GameObject(){ name = "User Study" };

        u.AddComponent<AudioSource>();
        _infoText = GameObject.FindGameObjectWithTag("HUD").transform.Find("StudyInfoText").GetComponentInChildren<Text>();
        _infoText.resizeTextForBestFit = true;

        _spawner = u.AddComponent<USActionSpawner>();
        _actionControl = u.AddComponent<USActionController>();
        u.AddComponent<USAction>();
    }

    private void RegisterListener()
    {
        _actionControl.SetTimeOutSound(TimeOutSound, TimeOutVolume);
        _actionControl.SetSuccessSound(SuccessSound, SuccessVolume);

        var action = GetComponent<USAction>();
        action.ActionStarted += _actionControl.OnActionStarted;
        action.ActionSuccess += _spawner.OnActionFinished;
        action.ActionSuccess += _actionControl.OnActionSuccess;
        action.ActionTimeOut += _spawner.OnActionFinished;
        action.ActionTimeOut += _actionControl.OnActionTimeOut;

        _spawner.ActionCountReached += OnRoundFinished;
        _mct.PlayerLeftTheBuilding += OnPlayerLeftTheBuilding;
    }

    private void StartLogging()
    {

    }

    private void FinishLogging()
    {

    }

    public void OnRoundFinished(object sender, EventArgs args)
    {
        Debug.Log("Round Finished");
        _roundFinished = true;
    }

    public string GetCurrentFeedbackType()
    {
        return "" + _currentFeedbackType;
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
