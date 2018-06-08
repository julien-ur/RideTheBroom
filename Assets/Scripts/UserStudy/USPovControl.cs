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
    private Vector3 _rotAxis;


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
        transform.rotation = _pc.transform.rotation;
        transform.Rotate(transform.InverseTransformVector(_rotAxis), 75);
        _posRelativeToPlayer = transform.forward * 50;

        transform.position = _pc.transform.position + _posRelativeToPlayer;
        
        //if (!_povActive)
        //{
        //    UpdateChildPositionAndRotation();
        //}
    }

    private void UpdateRelativePositionAndRotation()
    {
        Transform playerTrans = _pc.transform;

        if (PovPos == USTask.POSITION.Right)
        {
            _rotAxis = playerTrans.up;
        }
        else if (PovPos == USTask.POSITION.Left)
        {
            _rotAxis = -playerTrans.up;
        }
        else
        {
            _rotAxis = -playerTrans.right;
        }

        //transform.rotation = playerTrans.rotation;
        //transform.Rotate(transform.InverseTransformVector(_rotAxis), 75);
        ////transform.rotation *= Quaternion.AngleAxis(75, transform.InverseTransformVector(rotAxis));
        //_posRelativeToPlayer = transform.forward * 50;
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
