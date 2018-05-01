using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class USPovControl : MonoBehaviour
{
    public EventHandler PovSelected;

    public AudioClip SelectingSound { get; set; }
    public USTask.POSITION PovPos { get; set; }

    private GameObject _pc;
    private AudioSource _audioSource;
    private USPovObject _rndChosenPov;

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

        if (PovPos == USTask.POSITION.Right)
        {
            relPos = new Vector3(43, 1, -8);
            relRot = new Vector3(0, -90, 0);
        }
        else if (PovPos == USTask.POSITION.Left)
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
        _rndChosenPov.StartSelectionProcess(() =>
        {
            if (PovSelected != null)
                PovSelected(this, EventArgs.Empty);
        });
    }

    public void StopSelectionProcess()
    {
        _audioSource.Stop();
        _rndChosenPov.StopSelectionProcess();
    }
}
