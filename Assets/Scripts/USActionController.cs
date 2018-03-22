using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USActionController : MonoBehaviour
{
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

    public void OnActionStarted(object sender, USActionEventArgs args)
    {
        Debug.Log("Action Started " + args.ActionType + " " + args.Count);
        string feedbackData = _usc.GetFeedbackData(args.ActionType);

        if (feedbackData == null) return;

        _fsr.TempFeedbackChange(feedbackData);
    }

    public void OnActionSuccess(object sender, USActionEventArgs args)
    {
        Debug.Log("Action Success " + args.ActionType + " " + args.Count);
        _audioSource.PlayOneShot(_successSound, _successVolume);
        _scc.AddScore(5);
    }

    public void OnActionTimeOut(object sender, USActionEventArgs args)
    {
        Debug.Log("Action Timeout " + args.ActionType + " " + args.Count);
        _audioSource.PlayOneShot(_timeOutSound, _timeOutVolume);
        _scc.AddScore(-2);
    }

    public void SetSuccessSound(AudioClip c, float volume)
    {
        _successSound = c;
        _successVolume = volume;
    }

    public void SetTimeOutSound(AudioClip c, float volume)
    {
        _timeOutSound = c;
        _timeOutVolume = volume;
    }
}
