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
    private float _rotAngle;


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
        transform.rotation *= Quaternion.AngleAxis(_rotAngle, transform.InverseTransformVector(_rotAxis));
        _posRelativeToPlayer = transform.forward * 75;

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
            _rotAngle = 75;
        }
        else if (PovPos == USTask.POSITION.Left)
        {
            _rotAxis = playerTrans.up;
            _rotAngle = -75;
        }
        else
        {
            _rotAxis = playerTrans.right;
            _rotAngle = -40;
        }

        transform.rotation = playerTrans.rotation;
        //transform.Rotate(transform.InverseTransformVector(_rotAxis), 75);
        transform.rotation *= Quaternion.AngleAxis(_rotAngle, transform.InverseTransformVector(_rotAxis));
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
