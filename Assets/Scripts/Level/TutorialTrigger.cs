using UnityEngine;

public class TutorialTrigger : MonoBehaviour {

    public Constants.TUTORIAL_ACTION action;

    private Tutorial tutorial;
    private bool activated = false;

    private void Start()
    {
        tutorial = GameComponents.GetTutorial();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!activated && other.gameObject.tag == "Player")
        {
            tutorial.TriggerAction(action);
            activated = true;
        }
    }
}