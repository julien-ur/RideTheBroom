using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditorInternal;
using UnityEngine;

public struct USGameStatusLogRecord
{
    public int SubjectId { get; set; }
    public string Timestamp { get; set; }
    public string FeedbackType { get; set; }
    public float PlayerXPos { get; set; }
    public float PlayerYPos { get; set; }
    public float PlayerZPos { get; set; }
    public float HorizontalPlayerRot { get; set; }
    public float VerticalPlayerRot { get; set; }
    public float HorizontalHmdRot { get; set; }
    public float VerticalHmdRot { get; set; }

    public static string GetCSVHeader(char delimiter)
    {
        string csvString = typeof(USGameStatusLogRecord).GetProperties().Aggregate("", (current, property) => current + (property.Name + delimiter));
        return csvString.TrimEnd(delimiter);
    }

    public static string ConvertToCSVString(USGameStatusLogRecord self, char delimiter)
    {
        
        string csvString = typeof(USGameStatusLogRecord).GetProperties().Aggregate("", (current, property) => current + (property.GetValue(self, null) + "" + delimiter));
        return csvString.TrimEnd(delimiter);
    }
}

public struct USEventLogRecord
{
    public int SubjectId { get; set; }
    public string Timestamp { get; set; }
    public string EventType { get; set; }
    public string EventStatus { get; set; }
    public string EventInfo { get; set; }
    public float EventId { get; set; }

    public static string GetCSVHeader(char delimiter)
    {
        string csvString = typeof(USEventLogRecord).GetProperties().Aggregate("", (current, property) => current + (property.Name + delimiter));
        return csvString.TrimEnd(delimiter);
    }

    public static string ConvertToCSVString(USEventLogRecord self, char delimiter)
    {
        string csvString = typeof(USEventLogRecord).GetProperties().Aggregate("", (current, property) => current + (property.GetValue(self, null) + "" + delimiter));
        return csvString.TrimEnd(delimiter);
    }
}

public class USLogging : MonoBehaviour
{
    public int LogsPerSecond = 10;
    public char Delimiter = ';';
    public char NumberDecimalSeparator = '.';

    private UserStudyControl _usc;
    private Coroutine _loggingCoroutine;
    private Transform _playerTrans;
    private Transform _mainCamTrans;
    private USTask _lastSecondaryTask;


    private bool _logging;
    private float _loggingStartTime;
    private int _subjectId;

    private USTaskControllerEventArgs[] _taskData;


    void Start()
    {
        _usc = GameComponents.GetUserStudyControl();
        _playerTrans = GameComponents.GetPlayer().transform;
        _mainCamTrans = Camera.main.transform;
        _taskData = new USTaskControllerEventArgs[2];

        UserStudyControl.StudyStarted += StartLogging;
        UserStudyControl.StudyFinished += FinishLogging;

        var taskControl = GetComponent<USTaskController>();
        taskControl.TaskStarted += OnTaskStarted;
        taskControl.TaskEnded += OnTaskEnded;

        var emptyTaskData = new USTaskControllerEventArgs { Position = USTask.POSITION.None, EventInfo = "" };
        _taskData[0] = emptyTaskData;
        _taskData[1] = (USTaskControllerEventArgs)emptyTaskData.Clone();

        var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = NumberDecimalSeparator.ToString();
        Thread.CurrentThread.CurrentCulture = customCulture;
    }

    IEnumerator CSVLogger()
    {
        CreateEventLogCSV();

        using (StreamWriter gameStatusWriter = new StreamWriter("UserStudy/subject_" + _subjectId + "_gamestatus.csv"))
        {
            gameStatusWriter.WriteLine(USGameStatusLogRecord.GetCSVHeader(Delimiter));

            while (_logging)
            {
                float startTime = Time.realtimeSinceStartup;
                USGameStatusLogRecord record = CreateNewGameStatusRecord();
                gameStatusWriter.WriteLine(USGameStatusLogRecord.ConvertToCSVString(record, Delimiter));
                gameStatusWriter.Flush();
                float endTime = Time.realtimeSinceStartup;
                yield return new WaitForSecondsRealtime(1.0f / LogsPerSecond - (endTime - startTime));
            }
        }
    }

    private void CreateEventLogCSV()
    {
        StreamWriter eventWriter = new StreamWriter("UserStudy/subject_" + _subjectId + "_events.csv");
        eventWriter.WriteLine(USEventLogRecord.GetCSVHeader(Delimiter));
        eventWriter.Close();
    }

    private USGameStatusLogRecord CreateNewGameStatusRecord()
    {
        var record = new USGameStatusLogRecord()
        {
            SubjectId = _subjectId,
            Timestamp = (Time.realtimeSinceStartup - _loggingStartTime).ToString("F3"),
            FeedbackType = _usc.GetCurrentFeedbackType().ToString(),
            PlayerXPos = _playerTrans.position.x,
            PlayerYPos = _playerTrans.position.y,
            PlayerZPos = _playerTrans.position.z,
            HorizontalPlayerRot = _playerTrans.eulerAngles.y,
            VerticalPlayerRot = _playerTrans.eulerAngles.x,
            HorizontalHmdRot = _mainCamTrans.eulerAngles.y,
            VerticalHmdRot = _mainCamTrans.eulerAngles.x
        };

        return record;
    }

    private void WriteEventLogRecord(USTask.TYPE type, string status, string info, float id)
    {
        string t = (type == USTask.TYPE.Main) ? "Ring" : "POV";

        var record = new USEventLogRecord()
        {
            SubjectId = _subjectId,
            Timestamp = (Time.realtimeSinceStartup - _loggingStartTime).ToString("F3"),
            EventType = t,
            EventStatus = status,
            EventInfo = info,
            EventId = id
        };

        var eventWriter = new StreamWriter("UserStudy/subject_" + _subjectId + "_events.csv", true);
        eventWriter.WriteLine(USEventLogRecord.ConvertToCSVString(record, Delimiter));
        eventWriter.Close();
    }

    public void OnTaskStarted(object sender, USTaskControllerEventArgs args)
    {
        string info = (args.Type == USTask.TYPE.Main) ? args.Task.GetActiveRingPosition().ToString() : args.Position.ToString();
        WriteEventLogRecord(args.Type, "visible", info, args.Task.GetInstanceID());
    }

    public void OnTaskEnded(object sender, USTaskControllerEventArgs args)
    {
        WriteEventLogRecord(args.Type, args.EventInfo, "", args.Task.GetInstanceID());
    }

    public void StartLogging(object sender, UserStudyEventArgs e)
    {
        _subjectId = e.SubjectID;
        _logging = true;
        _loggingStartTime = Time.realtimeSinceStartup;
        _loggingCoroutine = StartCoroutine(CSVLogger());
    }

    public void FinishLogging(object sender, EventArgs eventArgs)
    {
        _logging = false;
        StopCoroutine(_loggingCoroutine);
    }
}