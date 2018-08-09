using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FeedbackUSB))]
[CanEditMultipleObjects]
public class FeedbackUSBEditor : Editor
{
    private FeedbackUSB _fusb;
    private UserStudyControl.FeedbackType _currFeedbackType;
    private bool _studyRunning;

    void Awake()
    {
        _fusb = GameComponents.GetFeedbackUSB();

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
        DrawFeedbackPresenter();
    }

    private void DrawFeedbackPresenter()
    {
        EditorGUILayout.LabelField("____________________________________________");
        EditorGUILayout.LabelField("FEEDBACK PRESENTER");

        int counter = 0;
        bool reachedCurrentType = false;
        foreach (var entry in UserStudyControl.FEEDBACK_DICT)
        {

            if (counter++ % 3 == 0) EditorGUILayout.LabelField("");

            if (_studyRunning && !reachedCurrentType && entry.Key.Contains("" + _currFeedbackType))
            {
                reachedCurrentType = true;
                EditorGUILayout.LabelField("- CURRENT TYPE -");
            }

            if (GUILayout.Button(entry.Key))
            {
                _fusb.UpdateFeedback(entry.Value + ";");
            }
        }

        EditorGUILayout.LabelField("");
    }
}