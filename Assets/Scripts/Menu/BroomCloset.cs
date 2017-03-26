using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomCloset : MonoBehaviour {

    public AnimationClip LandingMountainWorld;
    public AnimationClip LandingFloatingRocks;
    public AnimationClip OpenBarnDoors;

    private Animation anim;

    void Start()
    {
        anim = GetComponent<Animation>();
    }

    public float StartLanding()
    {
        Constants.LEVEL activeLevel = GameComponents.GetGameController().GetActiveLevel();

        if (activeLevel == Constants.LEVEL.Tutorial)
        {
            anim.clip = LandingMountainWorld;
        }
        else if (activeLevel == Constants.LEVEL.FloatingRocks)
        {
            anim.clip = LandingFloatingRocks;
        }

        anim.Play();
        return anim.clip.length;
    }

    public float OpenDoors()
    {
        anim.clip = OpenBarnDoors;
        anim.Play();
        return anim.clip.length;
    }
}
