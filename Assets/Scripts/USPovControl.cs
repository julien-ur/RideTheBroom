using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class USPovControl : MonoBehaviour {

    public AudioClip SelectingSound;

    private USAction.POSITION _povPos;
    private AudioSource _audioSource;
    private USPovObject _rndChosenPov;

    private GameObject _pc;
    private Vector3 _posRelativeToPlayer;

    void Awake()
    {
        _pc = GameComponents.GetPlayer();
    }

    void Start()
    {
        InitPovs();
        UpdateRelativePositionAndRotation();
    }

    private void InitPovs()
    {
        var availablePovObjects = GetComponentsInChildren<USPovObject>();
        int rndPovIndex = Random.Range(0, availablePovObjects.Length - 1);

        _rndChosenPov = availablePovObjects[rndPovIndex];
        _audioSource = GetComponentInChildren<AudioSource>();
    }

    void Update()
    {
        transform.position = _pc.transform.position + _posRelativeToPlayer;

        //if (!_povActive)
        //    UpdateChildPositionAndRotation();
    }

    private void UpdateRelativePositionAndRotation()
    {
        Transform playerTrans = _pc.transform;

        Vector3 relPos, relRot;

        if (_povPos == USAction.POSITION.Right)
        {
            relPos = new Vector3(43, 1, -8);
            relRot = new Vector3(0, -90, 0);
        }
        else if (_povPos == USAction.POSITION.Left)
        {
            relPos = new Vector3(-43, 1, -8);
            relRot = new Vector3(0, 90, 0);
        }
        else
        {
            relPos = new Vector3(0, 55, 55);
            relRot = new Vector3(40, 180, -18);
        }

        _posRelativeToPlayer = (relPos.x * playerTrans.right) + (relPos.y * playerTrans.up) + (relPos.z * playerTrans.forward);
        transform.rotation = playerTrans.rotation;
        transform.Rotate(relRot, Space.Self);
    }

    public void ActivateCurrentPov()
    {
        _rndChosenPov.Activate();
    }

    public void DeactivateCurrentPov()
    {
        _rndChosenPov.Deactivate();
    }

    public void StartSelectionProcess()
    {
        _audioSource.PlayOneShot(SelectingSound, 0.5f);
        _rndChosenPov.StartSelectionProcess();
    }

    public void StopSelectionProcess()
    {
        _audioSource.Stop();
        _rndChosenPov.StopSelectionProcess();
    }

    public void SetPos(USAction.POSITION pos)
    {
        _povPos = pos;
    }
}
