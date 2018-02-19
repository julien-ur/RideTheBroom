using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {

    public float spawnDistanceStart = 10;
    public float spawnDistanceEnd = 20;
    public float minSize = 20;
    public float maxSize = 100;
    public float obstacleCount = 300;
    public List<GameObject> obstacleObjects;

    private int counter = 0;
    private Transform playerTrans;
    private GameObject obstacleContainer;
    private List<GameObject> obstacles;
    
    /* TODO:
     *  Define Spawn Area (Cube around Player, larger than see distance)
     *  Randomly create game objects in area
     *  Update Spawn Area
     *  remove objects outside spawn area
     *  check object count and spawn new objects it is too low
     */

	void Start () {
        playerTrans = GameComponents.GetPlayer().transform;
        obstacleContainer = new GameObject();
        obstacleContainer.name = "ObstacleContainer";
        obstacles = new List<GameObject>();
        StartCoroutine(SpawnObstacles());
    }

    IEnumerator SpawnObstacles()
    {
        yield return new WaitForSeconds(2);

        while (obstacles.Count < obstacleCount)
        {
            SpawnNewObstacle(true);
        }

        while (true)
        {
            DestroyFarObstacles();

            while (obstacles.Count < obstacleCount)
            {
                SpawnNewObstacle();
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void SpawnNewObstacle(bool init=false)
    {
        GameObject o = obstacleObjects[Random.Range(0, obstacleObjects.Count)];

        // Source: https://answers.unity.com/answers/1324145/view.html
        Vector3 posInSphere = Random.insideUnitSphere;
        float length = posInSphere.magnitude;
        float ratioRadius = spawnDistanceStart / spawnDistanceEnd;
        Vector3 pos = (((1.0f - ratioRadius) * length + ratioRadius) / length) * spawnDistanceEnd * posInSphere;

        o.transform.position = pos + playerTrans.position;

        //float rndFwd = Random.Range(init ? -(spawnStartDistance + spawnRadius): spawnStartDistance, spawnStartDistance + spawnRadius);
        //float rndRight = Random.Range(-spawnRadius, spawnRadius);
        //float rndUp = Random.Range(-spawnRadius, spawnRadius);

        //o.transform.position = playerTrans.position + playerTrans.forward * rndFwd + playerTrans.up * rndUp + playerTrans.right * rndRight;

        o.transform.localScale = Vector3.one * Random.Range(minSize, maxSize);

        GameObject i = Instantiate(o);
        obstacles.Add(i);
        i.transform.parent = obstacleContainer.transform;

        Rigidbody rb = i.GetComponent<Rigidbody>();
        rb.AddTorque(Random.insideUnitSphere * 60);
        rb.AddForce(Random.insideUnitSphere * 60);
    }

    private void DestroyFarObstacles()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject o in obstacles)
        {
            if (Vector3.Distance(o.transform.position, playerTrans.position) > spawnDistanceEnd)
            {
                toRemove.Add(o);
            }
        }
        foreach (GameObject o in toRemove)
        {
            o.transform.parent = null;
            Destroy(o);
            obstacles.Remove(o);
        }
    }

}
