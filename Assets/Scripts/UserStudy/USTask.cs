using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class USTask : MonoBehaviour {

    public enum TYPE { Main, Secondary }
    public enum POSITION { Right, Middle, Left, None=-1 }

    public float MaxCompletionTimeMainTask = 5;
    public float MaxCompletionTimeSecondaryTask = 7;

    private UserStudyControl _usc;
    private Transform _playerTrans;

    private GameObject _taskItem;
    private bool _taskSuccess;

    void Awake()
    {
        _usc = GameComponents.GetLevelControl().GetComponent<UserStudyControl>();
        _playerTrans = GameComponents.GetPlayer().transform;
    }

    public void StartNewAction(TYPE type, POSITION pos, Action<bool> callback)
    {
        if (type == TYPE.Main)
            SpawnRings(pos);
        else 
            SpawnPov(pos);
        
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

    private void SpawnRings(POSITION activePos)
    {
        _taskItem = new GameObject() { name = "ActionRings" };
        var positions = GetPositionsExcluding(new[] { POSITION.None });

        foreach (POSITION pos in positions)
        {
            GameObject ringPrefab = (pos == activePos) ? _usc.RingObject : _usc.RingInactiveObject;
            Transform ringTrans = Instantiate(ringPrefab).transform;
            ringTrans.parent = _taskItem.transform;

            ringTrans.position = _playerTrans.position + 40 * _playerTrans.forward;

            int sideShift = 15;
            if (pos == POSITION.Left)
                ringTrans.position -= sideShift * _playerTrans.right;

            else if (pos == POSITION.Right)
                ringTrans.position += sideShift * _playerTrans.right;

            ringTrans.localRotation = _playerTrans.rotation;
            ringTrans.Rotate(new Vector3(-90, 0, 0));

            if (pos == activePos)
                ringTrans.GetComponent<USTaskTrigger>().TaskSuccess += OnTaskSuccess;
        }
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

    public void OnTaskSuccess(object sender, EventArgs args)
    {
        _taskSuccess = true;
    }

    public static POSITION[] GetPositionsExcluding(POSITION[] positionsToExclude)
    {
        return Enum.GetValues(typeof(POSITION))
            .Cast<POSITION>()
            .Where(pos => !positionsToExclude.Contains(pos))
            .ToArray();
    }
}
