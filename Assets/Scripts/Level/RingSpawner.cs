using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSpawner : MonoBehaviour {

    public float dist = 8;
    public float intervalMin = 2;
    public float intervalMax = 8;
    public float widthRadius = 15;
    public float heightRadius = 15;

    public GameObject ring;
    private GameObject ringContainer;
    private List<GameObject> rings;
    private Transform playerTrans;
    private Transform lastRingTrans;
    private float clearDistance;

    void Start () {
        playerTrans = GameComponents.GetPlayer().transform;
        ringContainer = new GameObject();
        ringContainer.name = "RingRoute";
        rings = new List<GameObject>();
        StartCoroutine(SpawnRings());
        clearDistance = dist * 1.5f;
    }

    private void SpawnRing()
    {
        Debug.Log("Spawn Ring");
        float rndFwd = dist;
        float rndRight = Random.Range(-widthRadius, widthRadius);
        float rndUp = Random.Range(-heightRadius / 1.5f, heightRadius);

        ring.transform.position = playerTrans.position + playerTrans.forward * dist + playerTrans.up * rndUp + playerTrans.right * rndRight;

        GameObject i = Instantiate(ring);
        rings.Add(i);

        i.transform.rotation = playerTrans.rotation;
        i.transform.Rotate(90, 0, 0);
        i.transform.parent = ringContainer.transform;

        lastRingTrans = i.transform;
    }

    private void DestroyDistantRings()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach(GameObject r in rings)
        {
            if (Vector3.Distance(r.transform.position, playerTrans.position) > clearDistance)
            {
                toRemove.Add(r);
            }
        }
        foreach (GameObject o in toRemove)
        {
            o.transform.parent = null;
            Destroy(o);
            rings.Remove(o);
        }
    }

    IEnumerator SpawnRings()
    {
        yield return new WaitForSeconds(2);

        while(true)
        {
            SpawnRing();
            DestroyDistantRings();
            yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));
        }
    }
}
