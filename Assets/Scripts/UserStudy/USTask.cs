using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class USTask : MonoBehaviour {


    public enum TYPE { Main, Secondary }
    public enum POSITION { Right, Middle, Left, None=-1 }

    private TYPE type;

    public float MaxCompletionTimeMainTask = 8;
    public float MaxCompletionTimeSecondaryTask = 5;

    private EventHandler MainTaskActivated;

    private UserStudyControl _usc;
    private PlayerControl _pc;
    private Transform _playerTrans;
    private const string ACTIVATION_TAG = "forActivation";

    private GameObject _taskItem;
    private Transform _activeRingTrans;
    private bool _activated;
    private bool _taskSuccess;
    private Action<bool> _successCallback;

    void Awake()
    {
        _usc = GameComponents.GetUserStudyControl();
        _pc = GameComponents.GetPlayerControl();
        _playerTrans = GameComponents.GetPlayer().transform;
    }

    public void StartNewTask(TYPE type, POSITION pos, Action<bool> callback)
    {
        this.type = type;
        _successCallback = callback;

        if (type == TYPE.Main)
        {
            //Spawn3Rings(pos);
            SpawnRing();
        }
        else
        {
            SpawnPov(pos);
        }

        StartCoroutine(CheckFullfilment(type, callback));
    }

    private IEnumerator CheckFullfilment(TYPE type, Action<bool> callback)
    {
        float timer = 0;
        float maxCompletionTime = type == TYPE.Main ? MaxCompletionTimeMainTask : MaxCompletionTimeSecondaryTask;

        while (!_taskSuccess)
        {
            if ((timer += Time.deltaTime) > maxCompletionTime)
            {
                callback(false);
                Destroy(_taskItem);
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }

        callback(true);
    }

    private void SpawnRing()
    {
        _activated = true;
        _taskItem = Instantiate(_usc.RingObject);

        Transform ringTrans = _taskItem.transform;
        ringTrans.position = _playerTrans.position + 80 * _playerTrans.forward;

        float widthRadius = 12;
        float heightRadius = 0;

        float rndRight = UnityEngine.Random.Range(-widthRadius, widthRadius);
        float rndUp = UnityEngine.Random.Range(-heightRadius, heightRadius);

        ringTrans.position += _playerTrans.up * rndUp + _playerTrans.right * rndRight;

        ringTrans.rotation = _playerTrans.rotation;
        ringTrans.Rotate(90, 0, 0);

        ringTrans.GetComponent<USTaskTrigger>().TaskSuccess += OnTaskSuccess;
        _activeRingTrans = ringTrans;
    }

    private void Spawn3Rings(POSITION activePos)
    {
        _taskItem = new GameObject() { name = "ActionRings" };
        var positions = GetPositionsExcluding(new[] { POSITION.None });

        foreach (POSITION pos in positions)
        {
            GameObject ringPrefab = _usc.RingInactiveObject;
            Transform ringTrans = Instantiate(ringPrefab).transform;
            ringTrans.parent = _taskItem.transform;

            ringTrans.position = _playerTrans.position + 80 * _playerTrans.forward;

            int sideShift = 20;
            if (pos == POSITION.Middle)
                ringTrans.position += sideShift/1.5f * _playerTrans.up;

            if (pos == POSITION.Left)
                ringTrans.position -= sideShift * _playerTrans.right;

            else if (pos == POSITION.Right)
                ringTrans.position += sideShift * _playerTrans.right;

            ringTrans.localRotation = _playerTrans.rotation;
            ringTrans.Rotate(new Vector3(-90, 0, 0));

            if (pos == activePos)
            {
                _activeRingTrans = ringTrans;
                ringTrans.gameObject.name = ACTIVATION_TAG;
            }
        }

        _pc.UpdateRotationScopeCenter();
    }

    private void SpawnPov(POSITION activePos)
    {
        _taskItem = new GameObject() { name = "ActionPov" };

        Transform povContainerTrans = Instantiate(_usc.PovContainer).transform;
        povContainerTrans.parent = _taskItem.transform;

        var povControl = _taskItem.AddComponent<USPovControl>();

        povControl.SelectingSound = _usc.PovSelectingSound;
        povControl.PovPos = activePos;
        povControl.PovSelected = OnTaskSuccess;
    }

    public void TryActivate()
    {
        if (_activated || type == TYPE.Secondary)
        {
            Debug.Log("Nothing to activate!");
            return;
        }

        GameObject ringToReplace = _taskItem.transform.Find(ACTIVATION_TAG).gameObject;
        GameObject activeRing = Instantiate(_usc.RingObject);
        activeRing.transform.parent = _taskItem.transform;
        activeRing.transform.position = ringToReplace.transform.position;
        activeRing.transform.rotation = ringToReplace.transform.rotation;
        activeRing.GetComponent<USTaskTrigger>().TaskSuccess += OnTaskSuccess;
        Destroy(ringToReplace);

        StartCoroutine(CheckFullfilment(type, _successCallback));
    }

    public void OnTaskSuccess(object sender, EventArgs args)
    {
        _taskSuccess = true;
    }

    public GameObject GetTaskObject()
    {
        return _taskItem;
    }
    
    public Vector3 GetActiveRingPosition()
    {
        return _activeRingTrans.position;
    }

    public static POSITION[] GetPositionsExcluding(POSITION[] positionsToExclude)
    {
        return Enum.GetValues(typeof(POSITION))
            .Cast<POSITION>()
            .Where(pos => !positionsToExclude.Contains(pos))
            .ToArray();
    }
}
