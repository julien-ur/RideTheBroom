using System;

public class USPovSelectable : VRSelectable
{
    public enum CONTAINER_TYPE { Visibility, Selection }
    public CONTAINER_TYPE ContainerType;

    private USPovControl _povControl;

    void Start()
    {
        _povControl = GetComponentInParent<USPovControl>();
    }

    public override void OnPlayerFocusEnter()
    {
        if (ContainerType == CONTAINER_TYPE.Visibility)
        {
            _povControl.ActivateCurrentPov();
        }
        else
        {
            _povControl.StartSelectionProcess();
        }
    }

    public override void OnPlayerFocusExit()
    {
        if (ContainerType == CONTAINER_TYPE.Selection)
        {
            _povControl.StopSelectionProcess();
        }
        else
        {
            _povControl.DeactivateCurrentPov();
        }
    }
}
