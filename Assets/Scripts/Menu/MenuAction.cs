using UnityEngine;

public class MenuAction : MonoBehaviour {

    [SerializeField] private ACTION VRAction;

    public enum ACTION { MainMenu, LevelMenu, Tutorial, FloatingRocks }

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
            case ACTION.MainMenu:
                //gc.MainMenu.SetActive(true);
                //gc.LevelMenu.SetActive(false);
                break;

            case ACTION.LevelMenu:
                //gc.MainMenu.SetActive(false);
                //gc.LevelMenu.SetActive(true);
                break;

            case ACTION.Tutorial:
                gc.StartTutorial();
                break;
        }
    }
}