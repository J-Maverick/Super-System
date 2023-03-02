
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SunKill : UdonSharpBehaviour
{
    public override void OnPlayerCollisionEnter(VRCPlayerApi player)
    {
        if (player.isLocal) player.Respawn();
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal) player.Respawn();
    }
}
