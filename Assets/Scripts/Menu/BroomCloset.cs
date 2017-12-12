using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomCloset : MonoBehaviour {

    public AnimationClip LandingMountainWorld;
    public AnimationClip LandingFloatingRocks;
    public AnimationClip LandingForrestCave;
    public AnimationClip OpenBarnDoors;
    public AudioSource barnDoorAudioSource;

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
        else if (activeLevel == Constants.LEVEL.ForestCave)
        {
            anim.clip = LandingForrestCave;
        }

        anim.Play();
        return anim.clip ? anim.clip.length : 0;
    }

    public float OpenDoors()
    {
        anim.clip = OpenBarnDoors;
        anim.Play();
        barnDoorAudioSource.Play();
        return anim.clip.length;
    }
}
