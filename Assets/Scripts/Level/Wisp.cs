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
    private List<Transform> usedWaypoints = new List<Transform>();
    private Transform target;
    private Transform nearestTargetToPlayer;

    private float speed;
    private int waypointCounter = 0;
    private double targetReachedDistance = 0.5;
    private float optimalPlayerDistance = 5;
    private float maxPlayerDistanceVariance = 15;
    private float maxSpeedChangeFactor = 3;

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
        target = FindNextTarget();
    }

    void Update()
    {
        if (!target) return;
        UpdateTarget();

        if (!target) return;

        Vector3 targetDir = (target.position - transform.position).normalized;

        transform.LookAt(playerTransform);
        transform.Translate(targetDir * speed * ComputePlayerDistanceFactor() * Time.deltaTime, Space.World);
    }

    private void UpdateTarget()
    {
        float targetDistance = (target.position - transform.position).magnitude;
        if (targetDistance < targetReachedDistance) target = FindNextTarget();
    }

    private Transform FindNextTarget()
    {
        float nearestDistance = float.MaxValue;
        Transform nextTarget = null;

        foreach(Transform waypoint in waypoints)
        {
            float distance = (waypoint.position - transform.position).magnitude;

            if (distance < nearestDistance)
            {
                if (usedWaypoints.Contains(waypoint)) continue;
                nearestDistance = distance;
                nextTarget = waypoint;
            }
        }

        usedWaypoints.Add(nextTarget);
        return nextTarget;
    }

    private float ComputePlayerDistanceFactor()
    {
        Vector3 relWispPos = playerTransform.InverseTransformPoint(transform.position);

        float optimalDistanceDelta = relWispPos.z - optimalPlayerDistance;

        float clampedDelta = Mathf.Clamp(optimalDistanceDelta, -maxPlayerDistanceVariance, maxPlayerDistanceVariance);
        float playerDistanceFactor = 1;

        if (optimalDistanceDelta > 0)
        {
            playerDistanceFactor = Utilities.Remap(clampedDelta, 0, maxPlayerDistanceVariance, 1, 1 / maxSpeedChangeFactor);
        }
        else
        {
            playerDistanceFactor = Utilities.Remap(clampedDelta, 0, -maxPlayerDistanceVariance, 1, maxSpeedChangeFactor);
        }

        return playerDistanceFactor;
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