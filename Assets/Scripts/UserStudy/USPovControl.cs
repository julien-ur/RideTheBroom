using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class USPovControl : MonoBehaviour
{
    public EventHandler FirstContact;
    public EventHandler PovSelected;

    public AudioClip SelectingSound { get; set; }
    public USTask.POSITION PovPos { get; set; }

    private GameObject _pc;
    private AudioSource _audioSource;
    private USPovObject _rndChosenPov;

    private Vector3 _posRelativeToPlayer;
    private Vector3 _rotAxis;
    private float _rotAngle;
    private bool _isFirstContact;

    void Awake()
    {
        _pc = GameComponents.GetPlayer();
    }

    void Start()
    {
        InitPovs();
        SetRotationAngle();
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
        transform.Rotate(transform.InverseTransformVector(GetRotationAxis()), _rotAngle);
        //transform.rotation *= Quaternion.AngleAxis(_rotAngle, transform.InverseTransformVector(_rotAxis));

        _posRelativeToPlayer = transform.forward * 75;
        transform.position = _pc.transform.position + _posRelativeToPlayer;
    }

    private void SetRotationAngle()
    {
        if (PovPos == USTask.POSITION.Right)
        {
            _rotAngle = 100;
        }
        else if (PovPos == USTask.POSITION.Left)
        {
            _rotAngle = -65;
        }
        else if(PovPos == USTask.POSITION.Middle)
        {
            _rotAngle = -40;
        }
    }

    private Vector3 GetRotationAxis()
    {
        return (PovPos == USTask.POSITION.Middle) ? _pc.transform.right : _pc.transform.up;
    }

    public void ActivateCurrentPov()
    {
        if (!_isFirstContact && FirstContact != null)
            FirstContact(this, EventArgs.Empty);
            _isFirstContact = true;

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
