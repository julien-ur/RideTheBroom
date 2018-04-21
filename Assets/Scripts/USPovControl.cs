using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class USPovControl : MonoBehaviour {

    public AudioClip SelectingSound;

    private Transform[] _povContainerTransforms;
    private AudioSource[] _audioSources;
    private USPovObject[] _rndChosenPovs;

    private GameObject _pc;
    private Vector3 _playerDeltaAtStart;

    private bool _povActive = false;

    void Awake()
    {
        _pc = GameComponents.GetPlayer();
        _playerDeltaAtStart = _pc.transform.position - transform.position;
        InitPovs();
        UpdateChildPositionAndRotation();
    }

    private void InitPovs()
    {
        _rndChosenPovs = new USPovObject[2];
        _audioSources = new AudioSource[2];
        _povContainerTransforms = new Transform[2];

        int rndPovIndex = 0;

        for (int i = 0; i < 2; i++)
        {
            Transform child = transform.GetChild(i);
            USPovSelectable.SIDE side = child.GetComponent<USPovSelectable>().Side;
            var povsForSide = child.GetComponentsInChildren<USPovObject>();

            if (i == 0) rndPovIndex = Random.Range(0, povsForSide.Length - 1);

            _rndChosenPovs[(int)side] = povsForSide[rndPovIndex];
            _audioSources[(int)side] = child.GetComponent<AudioSource>();
            _povContainerTransforms[(int)side] = child;
        }
    }

    void Update()
    {
        transform.position = _pc.transform.position - _playerDeltaAtStart;

        //if (!_povActive)
        //    UpdateChildPositionAndRotation();
    }

    private void UpdateChildPositionAndRotation()
    {
        Transform leftPovContainer = _povContainerTransforms[(int)USPovSelectable.SIDE.Left];
        Transform rightPovContainer = _povContainerTransforms[(int)USPovSelectable.SIDE.Right];
        Transform playerTrans = _pc.transform;

        Vector3 posWithoutSideAxes = playerTrans.position + (1 * playerTrans.up) + (-32 * playerTrans.forward);
        Vector3 leftSideShift = -43 * playerTrans.right;

        leftPovContainer.position = posWithoutSideAxes + leftSideShift;
        rightPovContainer.position = posWithoutSideAxes - leftSideShift;

        leftPovContainer.rotation = playerTrans.rotation;
        rightPovContainer.rotation = playerTrans.rotation;
    }

    public void ActivateCurrentPov(int side)
    {
        _povActive = true;
        _rndChosenPovs[side].Activate();
    }

    public void DeactivateCurrentPov(int side)
    {
        _rndChosenPovs[side].Deactivate(() => _povActive = false);
    }

    public void StartSelectionProcess(int side)
    {
        _audioSources[side].PlayOneShot(SelectingSound, 0.5f);

        if (_rndChosenPovs[side])
            _rndChosenPovs[side].StartSelectionProcess();
    }

    public void StopSelectionProcess(int side)
    {
        _audioSources[side].Stop();

        if (_rndChosenPovs[side])
            _rndChosenPovs[side].StopSelectionProcess();
    }
}
