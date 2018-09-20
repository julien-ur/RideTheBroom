using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FeedbackUSB))]
[CanEditMultipleObjects]
[ExecuteInEditMode]
public class FeedbackUSBEditor : Editor
{
    private FeedbackUSB _fusb;
    private UserStudyControl _usc;

    private UserStudyControl.FeedbackType _currFeedbackType;
    private bool _studyRunning;
    public int subjectId;
    public static int newSubjectId;
    public bool _useAutoId = false;


    void Awake()
    {
        _fusb = GameComponents.GetFeedbackUSB();
        _usc = GameComponents.GetUserStudyControl();

        UserStudyControl.StudyStarted += OnStudyStarted;
        UserStudyControl.RoundStarted += OnRoundStarted;
        UserStudyControl.StudyFinished += OnStudyFinished;
    }

    private void OnStudyStarted(object sender, UserStudyEventArgs e)
    {
        _currFeedbackType = e.FeedbackType;
        _studyRunning = true;
        Repaint();
    }

    private void OnRoundStarted(object sender, UserStudyEventArgs e)
    {
        _currFeedbackType = e.FeedbackType;
        Repaint();
    }

    public void OnStudyFinished(object sender, EventArgs e)
    {
        _studyRunning = false;
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DrawStudyControl();
        DrawFeedbackPresenter();
    }

    private void DrawStudyControl()
    {
        if (GUILayout.Button("Toggle Test HUD"))
        {
            _usc.TestHUD.SetActive(!_usc.TestHUD.activeSelf);
        }
        if (GUILayout.Button("Start Loading Scene"))
        {
            GameComponents.GetGameController().UnblockSceneLoading();
        }

        EditorGUILayout.LabelField("____________________________________________");
        EditorGUILayout.LabelField("STUDY CONTROL");
        
        EditorGUILayout.LabelField("Round: " +  _usc.GetCurrentRoundCount() + ", Repeat Count: " + _usc.GetCurrentRepeatCount());
        EditorGUILayout.LabelField("Round Type: " + _usc.GetRoundType());

        
        if (GUILayout.Button("Repeat Round: " + (UserStudyControl.IsRoundRepeated() ? "on" : "off")))
        {
            UserStudyControl.ToggleRoundRepeat();
        }
        if (GUILayout.Button("Force Finish Round"))
        {
            _usc.ForceFinishRound();
        }
        
        EditorGUILayout.LabelField("");

        if (GUILayout.Button("Start Feedback"))
        {
            foreach (var entry in UserStudyControl.DEFAULT_FEEDBACK)
            {
                _fusb.PermanentUpdate(entry.Key, entry.Value);
            }
        }
        if (GUILayout.Button("Stop Feedback"))
        {
            _fusb.StopAllFeedback();
        }
    }

    private void DrawFeedbackPresenter()
    {
        EditorGUILayout.LabelField("____________________________________________");
        EditorGUILayout.LabelField("FEEDBACK PRESENTER");

        int counter = 0;
        bool reachedCurrentType = false;
        foreach (var entry in UserStudyControl.FEEDBACK_DICT)
        {
            if (counter++ % 3 == 0 && counter > 1) EditorGUILayout.LabelField("");

            if (_studyRunning && !reachedCurrentType && entry.Key.Contains("" + _currFeedbackType))
            {
                reachedCurrentType = true;
                EditorGUILayout.LabelField("- CURRENT TYPE -");
            }

            if (GUILayout.Button(entry.Key))
            {
                _fusb.UpdateFeedback(entry.Value + ";");
                UserStudyControl.HasFeedbackPresented = true;
            }
        }

        EditorGUILayout.LabelField("");
    }
}