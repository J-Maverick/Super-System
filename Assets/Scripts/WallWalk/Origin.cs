
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cyan.PlayerObjectPool;

public class Origin : UdonSharpBehaviour
{
    public StationController playerStation;
    public CyanPlayerObjectAssigner pool;
    public UdonBehaviour playerAssignedPoolObject;

    public void _EnterRoom(Transform room)
    {
        transform.SetPositionAndRotation(room.position, room.rotation);
        playerStation._EnterStation();
    }

    public void _OnLocalPlayerAssigned()
    {
        playerStation = (StationController)pool._GetPlayerPooledUdon(Networking.LocalPlayer);
    }

    public void _OnPlayerAssigned()
    {
        ((StationController)(Component)playerAssignedPoolObject).origin = transform;
    }

}
