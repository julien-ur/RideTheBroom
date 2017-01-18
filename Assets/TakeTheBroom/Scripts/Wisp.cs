using UnityEngine;
using UnityEngine.AI;

public class Wisp : MonoBehaviour {

    public Transform[] waypoints;

    private NavMeshAgent agent;
    private Transform[] targets;
    private int actID;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void setTarget(int id)
    {
        actID = id;
        if (id >= waypoints.Length) return;
        agent.SetDestination(waypoints[id].position);
        
    }

    void Update()
    {
        Debug.Log(agent.remainingDistance + " " + actID);
       if(agent.remainingDistance < 10 && actID >= 1) 
       {
            actID++;
            agent.speed = 20;
            setTarget(actID);
       }
    }
}
