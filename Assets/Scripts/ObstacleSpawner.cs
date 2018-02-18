using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {

    public float spawnStartDistance = 10;
    public float spawnRadius = 10;
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

    private void Update() {

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
        // o.transform.position = (Random.insideUnitSphere * spawnRadius) + playerTrans.position + (Vector3.forward * spawnRadius/2);

        float rndFwd = Random.Range(init ? -(spawnStartDistance + spawnRadius): spawnStartDistance, spawnStartDistance + spawnRadius);
        float rndRight = Random.Range(-spawnRadius, spawnRadius);
        float rndUp = Random.Range(-spawnRadius, spawnRadius);

        o.transform.position = playerTrans.position + playerTrans.forward * rndFwd + playerTrans.up * rndUp + playerTrans.right * rndRight;
        if (o.name != "MagicalRing01")
            o.transform.localScale = Vector3.one * Random.Range(minSize, maxSize);
        else
            o.transform.eulerAngles = new Vector3(90, 0, 180);

        GameObject i = Instantiate(o);
        obstacles.Add(i);
        i.transform.parent = obstacleContainer.transform;
    }

    private void DestroyFarObstacles()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject o in obstacles)
        {
            if (Vector3.Distance(o.transform.position, playerTrans.position) > spawnStartDistance + spawnRadius)
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
