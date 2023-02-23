
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class StationTrigger : UdonSharpBehaviour
{
    public Origin origin;
    private bool initialized = false;

    public override void OnPlayerTriggerStay(VRCPlayerApi player)
    {
        //if (player.isLocal && player.isMaster && !origin.stationAssigned)
        //{
        //    Debug.Log("Adding local player to origin");
        //    origin.OnPlayerJoined(player);
        //}

        if (origin.stationAssigned && !initialized)
        {
            Debug.Log("Initializing origin");
            origin.Initialize();
            initialized = true;
        }
    }
}