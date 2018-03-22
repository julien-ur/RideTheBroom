using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UserStudyControl : MonoBehaviour
{

    public enum RoundType { Heat, Smell, Vibration }
    public string[] RoundNames = { "Wärme", "Geruch", "Vibration" };
    public RoundType[] Rounds = { RoundType.Smell, RoundType.Vibration, RoundType.Heat };

    public AudioClip TimeOutSound;
    public float TimeOutVolume = 0.3f;

    public AudioClip SuccessSound;
    public float SuccessVolume = 0.3f;

    private USActionSpawner _spawner;
    private Text _infoText;
    private Fading _fading;

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

        foreach (RoundType round in Rounds)
        {
            _roundFinished = false;
            _spawner.ResetActionCount();

            _fading.FadeOut(1, true);
            yield return new WaitForSecondsRealtime(1.5f);

            _infoText.text = RoundNames[(int)round];
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

        var action = u.AddComponent<USAction>();
        var actionControl = u.AddComponent<USActionController>();

        RegisterListener(action, actionControl);
    }

    private void RegisterListener(USAction action, USActionController actionControl)
    {
        actionControl.SetTimeOutSound(TimeOutSound, TimeOutVolume);
        actionControl.SetSuccessSound(SuccessSound, SuccessVolume);

        action.ActionStarted += actionControl.OnActionStarted;
        action.ActionSuccess += _spawner.OnActionFinished;
        action.ActionSuccess += actionControl.OnActionSuccess;
        action.ActionTimeOut += _spawner.OnActionFinished;
        action.ActionTimeOut += actionControl.OnActionTimeOut;

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
}
