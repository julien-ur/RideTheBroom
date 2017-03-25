using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GhostModeController : MonoBehaviour
{
	private const string GHOSTMODE_LOG_PATH = "ghostmodetest.txt";

    struct GhostModePathNode
    {
        public Vector3 position;
        public float time;
        public bool defined;

        public GhostModePathNode(Vector3 position, float time)
        {
            this.position = position;
            this.time = time;
            defined = true;
        }

        public GhostModePathNode(string logLine)
        {
        	string[] values = logLine.Split(' ');

        	time = System.Convert.ToSingle(values[0]);

        	position = new Vector3( System.Convert.ToSingle(values[1]), 
        							System.Convert.ToSingle(values[2]), 
        							System.Convert.ToSingle(values[3]));

        	defined = true;
        }

        public string ToString()
        {
            return time.ToString("F3") + " " + position.x.ToString("F3") + " " + position.y.ToString("F3") + " " + position.z.ToString("F3") + "\r\n";
        }
    }

    public Transform player;

    private List<GhostModePathNode> ghostModePathLog;
    private List<GhostModePathNode> ghostModePathLoaded;
    private float ghostModeLogTimer;
    private float lastGhostModeLogTime;
    private bool isGhostModeLogRunning;

    private GhostModePathNode previousNode;
    private GhostModePathNode nextNode;
    private int pathIndex;
    private bool isGhostMoving;

    private Transform ghost;

	void Start ()
	{
		BuildGhostPath();
		SpawnGhost();
	}
	
	void Update ()
	{
		HandleGhostModeLog();
		HandleGhostMovement();
	}

	private void HandleGhostModeLog()
    {
        if(isGhostModeLogRunning)
        {
            ghostModeLogTimer += Time.deltaTime;

            if(ghostModeLogTimer - lastGhostModeLogTime >= 1)
            {
                ghostModePathLog.Add(new GhostModePathNode(player.transform.position, ghostModeLogTimer));
                lastGhostModeLogTime = ghostModeLogTimer;
            }
        }
    }

    public void SaveGhostModeLog()
    {
        string text = "";

        foreach(GhostModePathNode node in ghostModePathLog)
        {
            text += node.ToString();
        }

        System.IO.File.WriteAllText(GHOSTMODE_LOG_PATH, text);
    }

    public void StartGhostModeLog(Transform player)
    {
    	this.player = player;
        ghostModePathLog = new List<GhostModePathNode>();
        isGhostModeLogRunning = true;
        ghostModeLogTimer = 0;
        lastGhostModeLogTime = 0;
    }

    public void StopGhostModeLog()
    {
        isGhostModeLogRunning = false;
    }

    private List<GhostModePathNode> LoadGhostModeLog(string filepath)
    {
    	string[] lines = System.IO.File.ReadAllLines(filepath);

    	List<GhostModePathNode> path = new List<GhostModePathNode>();

    	foreach(string line in lines)
    	{
    		path.Add(new GhostModePathNode(line));
    	}

    	return path;
    }

    private void BuildGhostPath()
    {
    	ghostModePathLoaded = LoadGhostModeLog("ghostmodetest.txt");

    	foreach(GhostModePathNode node in ghostModePathLoaded)
    	{
    		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    		cube.transform.position = node.position;
    	}
    }

    private void SpawnGhost()
    {
    	ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
    	isGhostMoving = true;
    }

    private void HandleGhostMovement()
    {
    	if(isGhostMoving)
    	{
	    	if(previousNode.defined == false) previousNode = ghostModePathLoaded[0];
	    	if(nextNode.defined == false) nextNode = ghostModePathLoaded[1];

	    	if(ghostModeLogTimer > nextNode.time)
	    	{
	    		foreach(GhostModePathNode node in ghostModePathLoaded)
		    	{
		    		if(ghostModeLogTimer < node.time)
		    		{
		    			previousNode = nextNode;
		    			nextNode = node;
		    			break;
		    		}
		    	}
	    	}

	    	if(nextNode.defined == false)
	    	{
	    		isGhostMoving = false;
	    		return;
	    	}

	    	float interpolant = (ghostModeLogTimer - previousNode.time) / (nextNode.time - previousNode.time);

	    	ghost.position = Vector3.Lerp(previousNode.position, nextNode.position, interpolant);
	    }
    }
}
