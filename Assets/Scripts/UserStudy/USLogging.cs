using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
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
    public string MainTaskXPos { get; set; }
    public string MainTaskYPos { get; set; }
    public string MainTaskZPos { get; set; }
    public float PlayerXPos { get; set; }
    public float PlayerYPos { get; set; }
    public float PlayerZPos { get; set; }
    public float HorizontalPlayerRot { get; set; }
    public float VerticalPlayerRot { get; set; }
    public float HorizontalHmdRot { get; set; }
    public float VerticalHmdRot { get; set; }

    public static string GetCSVHeader(char delimiter)
    {
        string csvString = typeof(USLogRecord).GetProperties().Aggregate("", (current, property) => current + (property.Name + delimiter));
        return csvString.TrimEnd(delimiter);
    }

    public static string ConvertToCSVString(USLogRecord self, char delimiter)
    {
        
        string csvString = typeof(USLogRecord).GetProperties().Aggregate("", (current, property) => current + (property.GetValue(self, null) + "" + delimiter));
        return csvString.TrimEnd(delimiter);
    }
}

public class USLogging : MonoBehaviour
{
    public int LogsPerSecond = 10;
    public char Delimiter = ';';
    public char NumberDecimalSeparator = ',';

    private UserStudyControl _usc;
    private Coroutine _loggingCoroutine;
    private Transform _playerTrans;
    private Transform _mainCamTrans;
    private Vector3 _activeRingPos;


    private bool _logging;
    private float _loggingStartTime;
    private string _subjectName;

    private USTaskControllerEventArgs[] _taskData;
    private CultureInfo _customCulture;


    void Start()
    {
        _usc = GameComponents.GetLevelControl().GetComponent<UserStudyControl>();
        _playerTrans = GameComponents.GetPlayer().transform;
        _mainCamTrans = Camera.main.transform;
        _taskData = new USTaskControllerEventArgs[2];

        var taskControl = GetComponent<USTaskController>();
        taskControl.TaskSpawned += OnTaskSpawned;
        taskControl.TaskStarted += OnTaskStarted;
        taskControl.TaskEnded += OnTaskEnded;

        var fsr = GameComponents.GetGameController().GetComponent<FeedbackServer>();
        fsr.FeedbackRequestSuccessful += OnFeedbackRequestSuccessful;

        var emptyTaskData = new USTaskControllerEventArgs { Position = USTask.POSITION.None, EventInfo = "" };
        _taskData[0] = emptyTaskData;
        _taskData[1] = (USTaskControllerEventArgs)emptyTaskData.Clone();

        var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = NumberDecimalSeparator.ToString();
        Thread.CurrentThread.CurrentCulture = customCulture;
    }

    IEnumerator CSVLogger()
    {
        using (StreamWriter sw = new StreamWriter("UserStudy/" + _subjectName + ".csv"))
        {
            sw.WriteLine(USLogRecord.GetCSVHeader(Delimiter));

            while (_logging)
            {
                float startTime = Time.realtimeSinceStartup;
                USLogRecord record = CreateNewRecord();
                sw.WriteLine(USLogRecord.ConvertToCSVString(record, Delimiter));
                sw.Flush();
                float endTime = Time.realtimeSinceStartup;
                yield return new WaitForSecondsRealtime(1.0f/LogsPerSecond - (endTime - startTime));
            }
        }
    }

    private USLogRecord CreateNewRecord()
    {
        USTaskControllerEventArgs _mainTaskData = _taskData[(int)USTask.TYPE.Main];
        USTaskControllerEventArgs _secondaryTaskData = _taskData[(int)USTask.TYPE.Secondary];

        bool mainTaskActive = _mainTaskData.Position != USTask.POSITION.None;
        bool secondaryTaskActive = _mainTaskData.Position != USTask.POSITION.None;

        var record = new USLogRecord()
        {
            SubjectName = _subjectName,
            Timestamp = (Time.realtimeSinceStartup - _loggingStartTime).ToString("F3"),
            FeedbackType = _usc.GetCurrentFeedbackType().ToString(),

            MainTask = mainTaskActive ? _mainTaskData.Position.ToString() : "",
            MainTaskInfo = _mainTaskData.EventInfo,
            SecondaryTask = secondaryTaskActive ? _secondaryTaskData.Position.ToString() : "",
            SecondaryTaskInfo = _secondaryTaskData.EventInfo,

            MainTaskXPos = mainTaskActive ? _activeRingPos.x.ToString() : "",
            MainTaskYPos = mainTaskActive ? _activeRingPos.y.ToString() : "",
            MainTaskZPos = mainTaskActive ? _activeRingPos.z.ToString() : "",
            PlayerXPos = _playerTrans.position.x,
            PlayerYPos = _playerTrans.position.y,
            PlayerZPos = _playerTrans.position.z,
            HorizontalPlayerRot = _playerTrans.eulerAngles.y,
            VerticalPlayerRot = _playerTrans.eulerAngles.x,
            HorizontalHmdRot = _mainCamTrans.eulerAngles.y,
            VerticalHmdRot = _mainCamTrans.eulerAngles.x
        };

        _mainTaskData.EventInfo = "";
        _secondaryTaskData.EventInfo = "";

        return record;
    }

    public void OnTaskSpawned(object sender, USTaskControllerEventArgs args)
    {
        _taskData[(int)args.Type].EventInfo = "Task Spawned";

        if (args.Type == USTask.TYPE.Main)
            _activeRingPos = args.Task.GetActiveRingPosition();
    }

    public void OnFeedbackRequestSuccessful(object sender, FeedbackServerEventArgs args)
    {
        _taskData[(int)USTask.TYPE.Secondary].EventInfo = args.EventInfo;
    }

    public void OnTaskStarted(object sender, USTaskControllerEventArgs args)
    {
        _taskData[(int)args.Type].Position = args.Position;
        _taskData[(int)args.Type].EventInfo = "Task Started";
    }

    public void OnTaskEnded(object sender, USTaskControllerEventArgs args)
    {
        _taskData[(int)args.Type].Position = USTask.POSITION.None;
        _taskData[(int)args.Type].EventInfo = args.EventInfo;
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