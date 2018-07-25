using System;
using UnityEngine;

public class USTaskTrigger : MonoBehaviour
{
    public EventHandler TaskSuccess;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            TriggerSuccess();
            if (transform.parent) Destroy(transform.parent.gameObject, 1f);
        }
    }

    public void TriggerSuccess()
    {
        if (TaskSuccess != null)
            TaskSuccess(this, EventArgs.Empty);
    }
}