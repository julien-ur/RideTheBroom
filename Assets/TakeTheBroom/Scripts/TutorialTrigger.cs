using UnityEngine;

public class TutorialTrigger : MonoBehaviour {

    public int id;

    private Tutorial tutorial;
    private bool activated = false;

    private void Start()
    {
        tutorial = GameObject.FindGameObjectWithTag("GameController").GetComponent<Tutorial>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!activated && other.gameObject.tag == "Player")
        {
            tutorial.trigger();
            activated = true;
        }
    }
}