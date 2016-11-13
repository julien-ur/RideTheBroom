using UnityEngine;
using System.Collections;

public class PS_Circle : MonoBehaviour {

    public int frequency = 1; // repeat rate
    public float resultion = 20; // amount of key on the created curve
    public float amplitude = 1.0f; // min/max height of the curve
    public float Zvalue = 0f; // for speed

	void Start () {
        CreateCircle();
	}
	
    void CreateCircle()
    {
        ParticleSystem PS = GetComponent<ParticleSystem>();
        var vel = PS.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        PS.startSpeed = 0f;
        vel.z = new ParticleSystem.MinMaxCurve(10.0f, Zvalue);

        AnimationCurve curveX = new AnimationCurve(); // create a new curve
        for(int i = 0; i < resultion; i++)
        {
            float newTime = (i / (resultion - 1));
            float newValue = amplitude * Mathf.Sin(newTime * (frequency * 2) * Mathf.PI);

            curveX.AddKey(newTime, newValue);
        }

        vel.x = new ParticleSystem.MinMaxCurve(10.0f, curveX);

        AnimationCurve curveY = new AnimationCurve(); // create a new curve
        for (int i = 0; i < resultion; i++)
        {
            float newTime = (i / (resultion - 1));
            float newValue = amplitude * Mathf.Cos(newTime * (frequency * 2) * Mathf.PI);

            curveY.AddKey(newTime, newValue);
        }

        vel.y = new ParticleSystem.MinMaxCurve(10.0f, curveY);
    }
}
