using UnityEngine;

public class Constants : MonoBehaviour
{
    // windzone
    public const float WINDZONE_STRENGTH = 50;
    public const float WINDZONE_COOLDOWN_TIME = 0.5f; // in sec

    // slowdown
    public const float SLOWDOWN_TIME = 0.3f; // in sec
    public const float SLOWDOWN_RECOVERY_TIME = 1f; // in sec
    public const float SLOWDOWN_TARGET_SPEED = 7f;

    // speed boost
    public const float SPEED_BOOST_TIME = 0.1f; // in sec
    public const float SPEED_BOOST_RECOVERY_TIME = 2f; // in sec
    public const float SPEED_BOOST_BOOST_SPEED = 35f;
}