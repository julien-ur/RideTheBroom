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
    public int Round { get; set; }
    public int RepeatCount { get; set; }
    public string RoundType { get; set; }
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
    public string RoundType { get; set; }
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

    private string _eventCsvPath;
    private string _gameStatusCsvPath;

    private USTaskControllerEventArgs[] _taskData;
    private bool _paused;
    private float _pauseStartTime;
    private float _pausedTime;


    void Start()
    {
        _usc = GameComponents.GetUserStudyControl();
        _playerTrans = GameComponents.GetPlayer().transform;
        _mainCamTrans = Camera.main.transform;
        _taskData = new USTaskControllerEventArgs[2];

        UserStudyControl.StudyStarted += InitLogging;
        UserStudyControl.StudyFinished += FinishLogging;

        UserStudyControl.RoundStarted += ResumeLogging;
        UserStudyControl.RoundFinished += PauseLogging;

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

    IEnumerator GameStatusLogger()
    {
        using (StreamWriter gameStatusWriter = new StreamWriter(_gameStatusCsvPath))
        {
            gameStatusWriter.WriteLine(USGameStatusLogRecord.GetCSVHeader(Delimiter));

            while (_logging)
            {
                yield return new WaitUntil(() => !_paused);
                float startTime = Time.realtimeSinceStartup;
                USGameStatusLogRecord record = CreateNewGameStatusRecord();
                gameStatusWriter.WriteLine(USGameStatusLogRecord.ConvertToCSVString(record, Delimiter));
                gameStatusWriter.Flush();
                float endTime = Time.realtimeSinceStartup;
                yield return new WaitForSecondsRealtime(1.0f / LogsPerSecond - (endTime - startTime));
            }
        }
    }

    private USGameStatusLogRecord CreateNewGameStatusRecord()
    {
        var record = new USGameStatusLogRecord()
        {
            SubjectId = _subjectId,
            Timestamp = GetFormattedTimestamp(),
            Round = _usc.GetCurrentRoundCount(),
            RepeatCount = _usc.GetCurrentRepeatCount(),
            RoundType = GetFormattedFeedbackType(),
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

    private void CreateEventLogCSV()
    {
        StreamWriter eventWriter = new StreamWriter(_eventCsvPath);
        eventWriter.WriteLine(USEventLogRecord.GetCSVHeader(Delimiter));
        eventWriter.Close();
    }

    private void WriteEventLogRecord(USTask.TYPE type, string status, string info, float id)
    {
        string t = (type == USTask.TYPE.Main) ? "Ring" : "POV";

        var record = new USEventLogRecord()
        {
            SubjectId = _subjectId,
            Timestamp = GetFormattedTimestamp(),
            RoundType = GetFormattedFeedbackType(),
            EventType = t,
            EventStatus = status,
            EventInfo = info,
            EventId = id
        };

        var eventWriter = new StreamWriter(_eventCsvPath, true);
        eventWriter.WriteLine(USEventLogRecord.ConvertToCSVString(record, Delimiter));
        eventWriter.Close();
    }

    private string GetFormattedTimestamp()
    {
        return (Time.realtimeSinceStartup - _loggingStartTime - _pausedTime).ToString("F3");
    }

    private string GetFormattedFeedbackType()
    {
        return _usc.GetRoundType().ToString();
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

    public void InitLogging(object sender, UserStudyEventArgs e)
    {
        if (_usc.GetCurrentRoundCount() > 0) return;
        _subjectId = e.SubjectID;
        _logging = true;
        _paused = true;

        GenerateSaveCsvPaths();

        _loggingCoroutine = StartCoroutine(GameStatusLogger());
        CreateEventLogCSV();
    }

    private void GenerateSaveCsvPaths()
    {
        _eventCsvPath = "UserStudy/s" + _subjectId + "_events.csv";
        int counter = 0;

        while (File.Exists(_eventCsvPath))
        {
            counter++;
            _eventCsvPath = "UserStudy/s" + _subjectId + "_events_" + counter + ".csv";
        }

        _gameStatusCsvPath = "UserStudy/s" + _subjectId + "_gamestatus.csv";
        counter = 0;
        while (File.Exists(_gameStatusCsvPath))
        {
            counter++;
            _gameStatusCsvPath = "UserStudy/s" + _subjectId + "_gamestatus_" + counter + ".csv";
        }
    }

    public void FinishLogging(object sender, EventArgs eventArgs)
    {
        _logging = false;
        StopCoroutine(_loggingCoroutine);
    }

    public void PauseLogging(object sender, EventArgs eventArgs)
    {
        _paused = true;
        _pauseStartTime = Time.realtimeSinceStartup;
    }

    public void ResumeLogging(object sender, UserStudyEventArgs userStudyEventArgs)
    {
        if (_usc.GetCurrentRoundCount() == 0)
            _loggingStartTime = Time.realtimeSinceStartup;
        else
            _pausedTime += Time.realtimeSinceStartup - _pauseStartTime;

        _paused = false;
    }
}