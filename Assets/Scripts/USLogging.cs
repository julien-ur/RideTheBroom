using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

public struct USLogRecord
{
    public string SubjectName { get; set; }
    public string Timestamp { get; set; }
    public string FeedbackType { get; set; }
    public string MainTask { get; set; }
    public string MainTaskInfo { get; set; }
    public string SecondaryTask { get; set; }
    public string SecondaryTaskInfo { get; set; }
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
    private Coroutine _loggingCoroutine;
    private Transform _playerTransform;
    private Transform _mainCameraTransform;

    private bool _logging;
    private float _loggingStartTime;
    private string _subjectName;

    private USTaskControllerEventArgs[] _taskInfo;

    void Start()
    {
        _usc = GameComponents.GetLevelControl().GetComponent<UserStudyControl>();
        _playerTransform = GameComponents.GetPlayer().transform;
        _mainCameraTransform = Camera.main.transform;
        _taskInfo = new USTaskControllerEventArgs[2];

        var taskControl = GetComponent<USTaskController>();
        taskControl.TaskStarted += OnTaskStarted;
        taskControl.TaskEnded += OnTaskEnded;

        var fsr = GameComponents.GetGameController().GetComponent<FeedbackServer>();
        fsr.FeedbackRequestSuccessful += OnFeedbackRequestSuccessful;

        var emptyTaskInfo = new USTaskControllerEventArgs { Position = USTask.POSITION.None, EventInfo = "" };
        _taskInfo[0] = emptyTaskInfo;
        _taskInfo[1] = emptyTaskInfo;
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
        USTaskControllerEventArgs _mainTaskInfo = _taskInfo[(int)USTask.TYPE.Main];
        USTaskControllerEventArgs _secondaryTaskInfo = _taskInfo[(int)USTask.TYPE.Secondary];

        var record = new USLogRecord()
        {
            SubjectName = _subjectName,
            Timestamp = (Time.realtimeSinceStartup - _loggingStartTime).ToString("F3"),
            FeedbackType = _usc.GetCurrentFeedbackType().ToString(),
            MainTask = _mainTaskInfo.Position.ToString(),
            MainTaskInfo = _mainTaskInfo.EventInfo,
            SecondaryTask = _secondaryTaskInfo.Position.ToString(),
            SecondaryTaskInfo = _secondaryTaskInfo.EventInfo,
            HorizontalPlayerRot = _playerTransform.eulerAngles.y,
            VerticalPlayerRot = _playerTransform.eulerAngles.x,
            HorizontalHmdRot = _mainCameraTransform.eulerAngles.y,
            VerticalHmdRot = _mainCameraTransform.eulerAngles.x
        };

        _mainTaskInfo.EventInfo = "";
        _secondaryTaskInfo.EventInfo = "";

        return record;
    }

    public void OnFeedbackRequestSuccessful(object sender, FeedbackServerEventArgs args)
    {
        _taskInfo[(int)USTask.TYPE.Secondary].EventInfo = args.EventInfo;
    }

    public void OnTaskStarted(object sender, USTaskControllerEventArgs args)
    {
        _taskInfo[(int)args.Type].Position = args.Position;
        _taskInfo[(int)args.Type].EventInfo = args.EventInfo;
    }

    public void OnTaskEnded(object sender, USTaskControllerEventArgs args)
    {
        _taskInfo[(int)args.Type].Position = USTask.POSITION.None;
        _taskInfo[(int)args.Type].EventInfo = args.EventInfo;
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