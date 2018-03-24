
using System.Collections.Generic;

public static class Constants
{
    public enum LEVEL { Menu, Tutorial, FloatingRocks, ForestCave, ImmersionTest, SpaceProcedural };
    public enum TUTORIAL_ACTION { Start, Rings, WindZone, SpeedBoost, SlowDown, HardRoute }

    // windzones
    public const float WINDZONE_STRENGTH = 50;
    public const float WINDZONE_MOMENTUM_LOSS_TIME = 0.5f; // in sec

    // slowdowns
    public const float SLOWDOWN_TIME = 0.3f; // in sec
    public const float SLOWDOWN_RECOVERY_TIME = 1f; // in sec
    public const float SLOWDOWN_TARGET_SPEED = 7f;
}