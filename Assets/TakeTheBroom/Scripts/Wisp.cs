using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wisp : MonoBehaviour {

    public Transform player;
    public GameObject path;
    public float maxSpeed = 10;
    public float slowDownFactor = 0.1f;

    private Rigidbody rb;
    private bool freezed = false;
    private float speed;
    private int actWaypoint = 0;
    private Transform[] waypoints;
    private Transform target;
    private double targetReachedDistance = 0.5;

    void Start()
    {
        List<Transform> transforms = new List<Transform>(path.GetComponentsInChildren<Transform>());
        transforms.Remove(transform);
        waypoints = transforms.ToArray();

        rb = GetComponent<Rigidbody>();
        speed = maxSpeed;

        nextTarget();
    }

    public void nextTarget()
    {
        actWaypoint++;
        target = waypoints[actWaypoint];
    }

    public void freeze(bool b)
    {
        freezed = b;
    }

    void Update()
    {
        if (freezed) return;

        // check target distance
        var targetDistance = (target.position - transform.position).magnitude;
        if (targetDistance < targetReachedDistance) nextTarget();

        // wait for player
        var distanceToPlayer = (player.position - transform.position).magnitude;
        if (distanceToPlayer > 10)
        {
            speed = Mathf.Max(0, speed - slowDownFactor);
        }
        else
        {
            speed = Mathf.Min(maxSpeed, speed + slowDownFactor/2);
        }
        
        // move in target direction
        Vector3 dir = (target.position - transform.position).normalized;
        rb.MovePosition(transform.position + dir * speed * Time.deltaTime);
    }
}