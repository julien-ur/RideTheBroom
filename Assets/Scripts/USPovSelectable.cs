using System;

public class USPovSelectable : VRSelectable
{
    public enum SIDE { Left, Right }
    public SIDE Side;

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
            _povControl.ActivateCurrentPov((int)Side);
        }
        else
        {
            _povControl.StartSelectionProcess((int)Side);
        }
    }

    public override void OnPlayerFocusExit()
    {
        if (ContainerType == CONTAINER_TYPE.Selection)
        {
            _povControl.StopSelectionProcess((int)Side);
        }
        else
        {
            _povControl.DeactivateCurrentPov((int)Side);
        }
    }
}
