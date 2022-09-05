
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class StationAssigner : UdonSharpBehaviour
{
    StationController[] stationControllers;
    [UdonSynced] int[] stationAssignmentIDs;
    [UdonSynced] int indPosition;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer.isMaster)
        {
        }
    }
}
