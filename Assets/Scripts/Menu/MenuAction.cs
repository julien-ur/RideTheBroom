using UnityEngine;

public class MenuAction : MonoBehaviour {

    [SerializeField] private ACTION VRAction;
    private enum ACTION { Tutorial, FloatingRocks }

    private GameController gc;
    private bool activated = false;

    void Start()
    {
        gc = GameComponents.GetGameController();
    }

    public void OnVRSelection()
    {
        switch (VRAction)
        {
            case ACTION.Tutorial:
                gc.StartTutorial();
                break;

            case ACTION.FloatingRocks:
                gc.LoadLevel(Constants.LEVEL.FloatingRocks);
                break;
        }
    }
}