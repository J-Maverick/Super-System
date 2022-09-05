
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CollisionScreamer : UdonSharpBehaviour
{

    public override void OnPlayerCollisionEnter(VRCPlayerApi player)
    {
        Debug.LogFormat("{0}: Player {1} collided with me!", name, player.displayName);
    }

    public override void OnPlayerCollisionExit(VRCPlayerApi player)
    {
        Debug.LogFormat("{0}: Player {1} stopped colliding with me!", name, player.displayName);
    }

    public override void OnPlayerCollisionStay(VRCPlayerApi player)
    {
        Debug.LogFormat("{0}: Player {1} is colliding with me!", name, player.displayName);
    }
}
