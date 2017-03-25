using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wisp : MonoBehaviour {

    public Transform player;
    public AudioClip introductionClip;
    public float maxSpeed = 18;
    public float defaultSpeed = 12;
    public float slowDownFactor = 0.1f;

    private AudioSource audioSource;
    private GameObject path;
    private Rigidbody rb;
    private float speed;
    private int actWaypoint = 0;
    private Transform[] waypoints;
    private Transform target;
    private double targetReachedDistance = 0.5;

    private bool speedLocked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        speed = 0;
    }

    public void initWaypoints()
    {
        path = GameObject.Find("Wisp Waypoints");
        List<Transform> transforms = new List<Transform>(path.GetComponentsInChildren<Transform>());
        transforms.Remove(transform);
        waypoints = transforms.ToArray();
        
        nextTarget();
    }

    public void nextTarget()
    {
        actWaypoint++;
        target = waypoints[actWaypoint];
    }

    void Update()
    {
        if (!target) return;

        // check target distance
        var targetDistance = (target.position - transform.position).magnitude;
        if (targetDistance < targetReachedDistance) nextTarget();

        // wait for player
        //if(!speedLocked)
        //{
        //    var distanceToPlayer = (player.position - transform.position).magnitude;
        //    if (distanceToPlayer > 10)
        //    {
        //        speed = Mathf.Max(0, speed - slowDownFactor);
        //    }
        //    else
        //    {
        //        speed = Mathf.Min(maxSpeed, speed + slowDownFactor / 2);
        //    }
        //}

        // move in target direction
        Vector3 dir = (target.position - transform.position).normalized;
        rb.MovePosition(transform.position + dir * speed * Time.deltaTime);
    }

    public void startFlying()
    {
        changeSpeedToTargetSpeed(defaultSpeed, 2);
    }

    public void changeSpeedToTargetSpeed(float targetSpeed, float duration)
    {
        float startSpeed = speed;
        speedLocked = true;

        if (duration == 0) speed = targetSpeed;
        else StartCoroutine(adjustSpeed(targetSpeed, startSpeed, duration, true));
    }

    public void changeSpeedToDefaultSpeed(float duration)
    {
        float startSpeed = speed;
        StartCoroutine(adjustSpeed(defaultSpeed, startSpeed, duration, false));
    }

    IEnumerator adjustSpeed(float targetSpeed, float startSpeed, float duration, bool b)
    {
        yield return new WaitUntil(() => adjustSpeedToTargetSpeed(targetSpeed, startSpeed, duration));
        speedLocked = b;
    }

    private bool adjustSpeedToTargetSpeed(float targetSpeed, float startSpeed, float duration)
    {
        speed += ((targetSpeed - startSpeed) / duration) * Time.deltaTime;
        bool slowDown = (targetSpeed < startSpeed);

        if (speed <= targetSpeed && slowDown || speed >= targetSpeed && !slowDown)
        {
            speed = targetSpeed;
            return true;
        }
        else return false;
    }

    public void talkToPlayer(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}