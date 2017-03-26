using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wisp : MonoBehaviour {

    public AudioClip introductionClip;
    public float defaultSpeed = 12;

    private AudioSource audioSource;
    private Transform playerTransform;
    private Transform[] waypoints;
    private Transform target;

    private float speed;
    private int waypointCounter = 0;
    private double targetReachedDistance = 0.5;
    private float optimalPlayerDistance = 2;
    private float maxPlayerDistanceVariance = 3;
    private float maxSpeedChangeFactor = 2;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerTransform = GameComponents.GetPlayer().transform;
        speed = 0;
    }

    public void initWaypoints()
    {
        GameObject wpContainer = GameObject.Find("Wisp Waypoints");
        waypoints = GameComponents.GetComponentsInChildrenWithoutParent<Transform>(wpContainer);
        target = waypoints[waypointCounter++];
    }

    void Update()
    {
        if (!target) return;
        UpdateTarget();

        Vector3 dir = (target.position - transform.position).normalized;

        //float playerDistance = Vector3.Distance(playerTransform.position, transform.position);
        //float optimalPlayerDistanceDelta = playerDistance - optimalPlayerDistance;
        //float clampedDelta = Mathf.Clamp(optimalPlayerDistanceDelta + maxPlayerDistanceVariance, 0, 2 * maxPlayerDistanceVariance);
        //float playerDistanceFactor = Utilities.Remap(clampedDelta, 0, 2 * maxPlayerDistanceVariance, maxSpeedChangeFactor, 1 / maxSpeedChangeFactor);

        //Debug.Log(playerDistance + " " + optimalPlayerDistanceDelta + " " + clampedDelta + " " + playerDistanceFactor);

        transform.Translate(dir * speed * Time.deltaTime);
    }

    private void UpdateTarget()
    {
        float targetDistance = (target.position - transform.position).magnitude;
        if (targetDistance < targetReachedDistance) target = waypoints[waypointCounter++];
    }

    public void startFlying()
    {
        changeSpeedToTargetSpeed(defaultSpeed, 2);
    }

    public void talkToPlayer(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void changeSpeedToTargetSpeed(float targetSpeed, float duration)
    {
        float startSpeed = speed;

        if (duration == 0) speed = targetSpeed;
        else StartCoroutine(adjustSpeed(targetSpeed, startSpeed, duration));
    }

    public void changeSpeedToDefaultSpeed(float duration)
    {
        float startSpeed = speed;
        StartCoroutine(adjustSpeed(defaultSpeed, startSpeed, duration));
    }

    IEnumerator adjustSpeed(float targetSpeed, float startSpeed, float duration)
    {
        yield return new WaitUntil(() => adjustSpeedToTargetSpeed(targetSpeed, startSpeed, duration));
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

    
}