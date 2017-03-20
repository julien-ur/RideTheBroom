using UnityEngine;

public class MenuAction : MonoBehaviour {

    [SerializeField] private ACTION VRAction;

    public enum MENU { MainMenu, LevelMenu };
    private enum ACTION { MainMenu, LevelMenu, Tutorial, FloatingRocks }
    private GameController gc;


    void Start()
    {
        //GetComponent<SelectionSlider>().OnBarFilled += OnVRSelection;
        gc = GameObject.FindGameObjectWithTag("GameControl").GetComponent<GameController>();
    }

    private void OnVRSelection()
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
                Destroy(GameObject.Find("Menu"));
                gc.LoadLevel(Constants.LEVEL.Tutorial);
                break;
        }
    }
}