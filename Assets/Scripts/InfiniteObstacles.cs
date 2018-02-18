using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteObstacles : MonoBehaviour {

    private Transform tx;
    private ParticleSystem.Particle[] obstacles;

    public int obstaclesMax = 100;
    public float obstacleSize = 1;
    public float obstacleDistance = 100;
    public float obstacleDistanceStart = 50;
    private float sqrObstacleDistance;

    void Start () {
        tx = transform;
        sqrObstacleDistance = obstacleDistance * obstacleDistance;

    }

    private void CreateObstacles()
    {
        obstacles = new ParticleSystem.Particle[obstaclesMax];

        for (int i = 0; i < obstaclesMax; i++)
        {
            obstacles[i].position = Random.insideUnitSphere * obstacleDistance + tx.position;
            obstacles[i].startSize = obstacleSize;
        }
    }
	
	void Update () {
        if (obstacles == null) CreateObstacles();

        for (int i = 0; i < obstaclesMax; i++)
        {
            if ((obstacles[i].position - tx.position).sqrMagnitude > sqrObstacleDistance)
            {
                obstacles[i].position = Random.insideUnitSphere * obstacleDistance + tx.position;
            }
        }

        GetComponent<ParticleSystem>().SetParticles(obstacles, obstacles.Length);
	}
}
