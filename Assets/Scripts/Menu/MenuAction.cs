using UnityEngine;

public class MenuAction : MonoBehaviour {

    [SerializeField] private ACTION VRAction;
    private enum ACTION { Tutorial, FloatingRocks, ForestCave, ImmersionTest }

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
                // gc.StartTutorial();
                gc.LoadLevel(Constants.LEVEL.Tutorial);
                break;

            case ACTION.FloatingRocks:
                gc.LoadLevel(Constants.LEVEL.FloatingRocks);
                break;
            case ACTION.ForestCave:
                gc.LoadLevel(Constants.LEVEL.ForestCave);
                break;
            case ACTION.ImmersionTest:
                gc.LoadLevel(Constants.LEVEL.ImmersionTest);
                break;
        }
    }
}