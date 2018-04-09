using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

public struct USLogRecord
{
    public string SubjectName { get; set; }
    public string Timestamp { get; set; }
    public string FeedbackType { get; set; }
    public string ActionType { get; set; }
    public string EventInfo { get; set; }
    public float HorizontalPlayerRot { get; set; }
    public float VerticalPlayerRot { get; set; }
    public float HorizontalHmdRot { get; set; }
    public float VerticalHmdRot { get; set; }

    public static string GetCSVHeader()
    {
        string csvString = typeof(USLogRecord).GetProperties().Aggregate("", (current, property) => current + (property.Name + ","));
        return csvString.TrimEnd(',');
    }

    public static string ConvertToCSVString(USLogRecord self)
    {
        string csvString = typeof(USLogRecord).GetProperties().Aggregate("", (current, property) => current + (property.GetValue(self, null) + ","));
        return csvString.TrimEnd(',');
    }
}

public class USLogging : MonoBehaviour
{
    public int LogsPerSecond = 10;

    private UserStudyControl _usc;
    private USAction _action;
    private Coroutine _loggingCoroutine;
    private Transform _playerTransform;
    private Transform _mainCameraTransform;

    private bool _logging;
    private float _loggingStartTime;
    private string _subjectName;
    private string _latestEvent;

    void Start()
    {
        _playerTransform = GameComponents.GetPlayer().transform;
        _mainCameraTransform = Camera.main.transform;
        _usc = GameComponents.GetLevelControl().GetComponent<UserStudyControl>();
        _action = GetComponent<USAction>();
    }

    IEnumerator CSVLogger()
    {
        using (StreamWriter sw = new StreamWriter("UserStudy/" + _subjectName + ".csv"))
        {
            sw.WriteLine(USLogRecord.GetCSVHeader());

            while (_logging)
            {
                float startTime = Time.realtimeSinceStartup;
                USLogRecord record = CreateNewRecord();
                sw.WriteLine(USLogRecord.ConvertToCSVString(record));
                sw.Flush();
                float endTime = Time.realtimeSinceStartup;
                yield return new WaitForSecondsRealtime(1.0f/LogsPerSecond - (endTime - startTime));
            }
        }
    }

    private USLogRecord CreateNewRecord()
    {
        var record = new USLogRecord()
        {
            SubjectName = _subjectName,
            Timestamp = (Time.realtimeSinceStartup - _loggingStartTime).ToString("F3"),
            FeedbackType = _usc.GetCurrentFeedbackType().ToString(),
            ActionType = _action.IsActionRunning() ? _action.GetCurrentActionName() : "No Action",
            EventInfo = _latestEvent ?? "no event",
            HorizontalPlayerRot = _playerTransform.eulerAngles.y,
            VerticalPlayerRot = _playerTransform.eulerAngles.x,
            HorizontalHmdRot = _mainCameraTransform.eulerAngles.y,
            VerticalHmdRot = _mainCameraTransform.eulerAngles.x
        };

        _latestEvent = null;

        return record;
    }

    public void OnRelevantStudyEvent(object sender, UserStudyControlEventArgs args)
    {
        _latestEvent = args.EventInfo;
    }

    public void StartLogging(string subjectName)
    {
        _subjectName = subjectName;
        _logging = true;
        _loggingStartTime = Time.realtimeSinceStartup;
        _loggingCoroutine = StartCoroutine(CSVLogger());
    }

    public void FinishLogging()
    {
        _logging = false;
        StopCoroutine(_loggingCoroutine);
    }
}