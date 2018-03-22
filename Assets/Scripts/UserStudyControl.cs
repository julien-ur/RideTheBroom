using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserStudyControl : MonoBehaviour
{

    public enum FeedbackType { Heat, Smell, Vibration }
    public string[] FeedbackLabels = { "Wärme", "Geruch", "Vibration" };
    public FeedbackType[] Rounds = { FeedbackType.Smell, FeedbackType.Vibration, FeedbackType.Heat };

    public static Dictionary<string, string> FEEDBACK_DICT = new Dictionary<string, string>
    {
        { "" + FeedbackType.Heat + USAction.TYPE.RefillTank, FeedbackServer.HEAT_TAG + "0, 2"},
        { "" + FeedbackType.Heat + USAction.TYPE.Accelerate, FeedbackServer.HEAT_TAG + "1, 2"},
        { "" + FeedbackType.Heat + USAction.TYPE.POV, FeedbackServer.HEAT_TAG + "1, 0.5"},

        { "" + FeedbackType.Smell + USAction.TYPE.RefillTank, FeedbackServer.SMELL_TAG + Constants.SMELL_WOODY + ", 2"},
        { "" + FeedbackType.Smell + USAction.TYPE.Accelerate, FeedbackServer.SMELL_TAG + Constants.SMELL_LEMON + ", 2"},
        { "" + FeedbackType.Smell + USAction.TYPE.POV, FeedbackServer.SMELL_TAG + Constants.SMELL_BERRY + ", 2"},

        { "" + FeedbackType.Vibration + USAction.TYPE.RefillTank, FeedbackServer.VIBRATION_TAG + "0.4, 2"},
        { "" + FeedbackType.Vibration + USAction.TYPE.Accelerate, FeedbackServer.VIBRATION_TAG + "1, 2"},
        { "" + FeedbackType.Vibration + USAction.TYPE.POV, FeedbackServer.VIBRATION_TAG + "0.4, 0.3;" + 
                                                           FeedbackServer.PAUSE_TAG + "0.5;" + 
                                                           FeedbackServer.VIBRATION_TAG + "0.4, 0.3;" + 
                                                           FeedbackServer.PAUSE_TAG + " 0.5p;" + 
                                                           FeedbackServer.VIBRATION_TAG + "0.4, 0.3"}
    };

    public AudioClip TimeOutSound;
    public float TimeOutVolume = 0.3f;

    public AudioClip SuccessSound;
    public float SuccessVolume = 0.3f;

    private USActionSpawner _spawner;
    private USActionController _actionControl;
    private Text _infoText;
    private Fading _fading;

    private FeedbackType _currentFeedbackType;
    private bool _roundFinished;

    private void Start()
    {
        _fading = GameComponents.GetFading();

        StartCoroutine(StartStudy());
    }

    IEnumerator StartStudy()
    {
        // yield return new WaitUntil(IsPlayerReady);
        yield return new WaitForSeconds(5);
        CreateStudyObject();

        Debug.Log("Study Started");

        foreach (FeedbackType f in Rounds)
        {
            _roundFinished = false;
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

    private void CreateStudyObject()
    {
        var u = new GameObject(){ name = "User Study" };

        u.AddComponent<AudioSource>();
        _infoText = GameObject.FindGameObjectWithTag("HUD").transform.Find("StudyInfoText").GetComponentInChildren<Text>();
        _infoText.resizeTextForBestFit = true;

        _spawner = u.AddComponent<USActionSpawner>();
        _actionControl = u.AddComponent<USActionController>();
        var action = u.AddComponent<USAction>();

        RegisterListener(action);
    }

    private void RegisterListener(USAction action)
    {
        _actionControl.SetTimeOutSound(TimeOutSound, TimeOutVolume);
        _actionControl.SetSuccessSound(SuccessSound, SuccessVolume);

        action.ActionStarted += _actionControl.OnActionStarted;
        action.ActionSuccess += _spawner.OnActionFinished;
        action.ActionSuccess += _actionControl.OnActionSuccess;
        action.ActionTimeOut += _spawner.OnActionFinished;
        action.ActionTimeOut += _actionControl.OnActionTimeOut;

        _spawner.ActionCountReached += OnRoundFinished;
    }

    private void StartLogging()
    {

    }

    private void FinishLogging()
    {

    }

    private bool IsPlayerReady()
    {
        return true;
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
