using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FeedbackUSB))]
[CanEditMultipleObjects]
public class FeedbackUSBEditor : Editor
{
    private FeedbackUSB _fusb;
    private UserStudyControl _usc;
    private USTaskSpawner _spawner;
    
    private UserStudyControl.FeedbackType _currFeedbackType;
    private bool _studyRunning;
    private static int newSubjectId;
    private bool _useAutoId = false;


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
        EditorGUILayout.LabelField("____________________________________________");
        EditorGUILayout.LabelField("STUDY CONTROL");
        
        EditorGUILayout.IntField("Auto Subject ID", _usc.GetAutoSubjectId());
        if (!_useAutoId)
        {
            if ((newSubjectId = EditorGUILayout.IntField("Subject ID", _usc.GetSubjectId())) != _usc.GetSubjectId())
            {
                _usc.SetSubjectId(newSubjectId);
            }
        }
        
        _useAutoId = EditorGUILayout.Toggle("Use Auto ID", _useAutoId);

        
        EditorGUILayout.LabelField("");
        
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

        if (GUILayout.Button("Pause Feedback"))
        {
            _fusb.StopAllFeedback();
        }
        if (GUILayout.Button("Resume Feedback"))
        {
            foreach (var entry in UserStudyControl.DEFAULT_FEEDBACK)
            {
                _fusb.PermanentUpdate(entry.Key, entry.Value);
            }
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