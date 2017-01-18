using UnityEngine;

public class TutorialTrigger : MonoBehaviour {

    public int id;

    private Tutorial tutorial;

    private void Start()
    {
        tutorial = GameObject.FindGameObjectWithTag("GameController").GetComponent<Tutorial>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            tutorial.trigger(id);
        }
    }
}
