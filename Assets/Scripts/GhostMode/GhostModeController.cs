using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GhostModeController : MonoBehaviour
{
	private string GHOSTMODE_LOG_PATH;

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

    public GameObject ghostPrefab;

    private Transform player;
    private List<GhostModePathNode> ghostModePathLog;
    private List<GhostModePathNode> ghostModePathLoaded;
    private float ghostModeLogTimer;
    private float lastGhostModeLogTime;
    private bool isGhostModeLogRunning;
    private bool isFirstRecord;

    private GhostModePathNode previousNode;
    private GhostModePathNode nextNode;
    private int pathIndex;
    private bool isGhostMoving;

    private Transform ghost;

	void Start ()
	{
        //GHOSTMODE_LOG_PATH = "ghostmodetest_" + GameObject.Find("LevelControl").GetComponent<LevelActions>().LEVEL_NAME + ".txt";
        GHOSTMODE_LOG_PATH = "ghostmodetest_" + GameComponents.GetGameController().GetActiveLevel().ToString() + ".txt";

        player = GameComponents.GetPlayer().transform;
        if (!File.Exists(GHOSTMODE_LOG_PATH))
		{
			isFirstRecord = true;
		}
		else
		{
			ghostModePathLoaded = LoadGhostModeLog(GHOSTMODE_LOG_PATH);
			//BuildGhostPath();
			SpawnGhost();
		}
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

            if(ghostModeLogTimer > 600) AbortGhostModeLog();
        }
    }

    public void SaveGhostModeLog()
    {
    	if(ghostModePathLog != null && (isFirstRecord || ghostModePathLoaded[ghostModePathLoaded.Count].time > ghostModeLogTimer))
    	{
	        string text = "";

	        foreach(GhostModePathNode node in ghostModePathLog)
	        {
	            text += node.ToString();
	        }

	        System.IO.File.WriteAllText(GHOSTMODE_LOG_PATH, text);
	    }
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

    public void AbortGhostModeLog()
    {
    	Debug.Log("Logging aborted");
    	isGhostModeLogRunning = false;
    	ghostModePathLog = null;
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
    	foreach(GhostModePathNode node in ghostModePathLoaded)
    	{
    		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    		cube.transform.position = node.position;
    	}
    }

    private void SpawnGhost()
    {
    	if(!isFirstRecord)
    	{
	    	if(ghostPrefab == null) ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
	    	else ghost = Instantiate(ghostPrefab, Vector3.zero, Quaternion.identity).transform;
	    	isGhostMoving = true;
	    }
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

	    	if(ghostModeLogTimer > nextNode.time)
	    	{
	    		isGhostMoving = false;
	    		return;
	    	}

	    	float interpolant = (ghostModeLogTimer - previousNode.time) / (nextNode.time - previousNode.time);

	    	ghost.position = Vector3.Lerp(previousNode.position, nextNode.position, interpolant);
	    }
    }
}
