
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Collections;
using VRC.SDK3.Components;

public class Origin : UdonSharpBehaviour
{
    public StationController playerStation;
    public Transform currentRoom;
    public Transform homeRoom;
    public bool stationAssigned = false;
    public Room[] rooms = null;

    public VRCObjectPool objectPool;

    public void Initialize()
    {
        _EnterRoom(homeRoom);
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (player.isLocal && stationAssigned)
        {
            _EnterRoom(currentRoom);
        }
    }

    public void AddRoom(Room room)
    {
        if (rooms == null)
        {
            rooms = new Room[1];
            rooms[0] = room;
        }
        else
        {
            Room[] tempList = new Room[rooms.Length + 1];
            for (int i = 0; i < tempList.Length; i++)
            {
                if (i < tempList.Length - 1)
                {
                    tempList[i] = rooms[i];
                }
                else tempList[i] = room;
            }
            rooms = tempList;
        }

    }

    public Transform GetRoomByID(int iD)
    {
        foreach (Room room in rooms)
        {
            Debug.LogFormat("{0}: Checking room {1} against ID {2}", name, room.roomID, iD);
            if (room.roomID == iD)
            {
                Debug.LogFormat("{0}: Found room with ID {1} -- returning {2}", name, iD, room);
                return room.transform;
            }
        }
        return null;
    }

    public void _EnterRoom(Transform room)
    {
        if (playerStation != null)
        {
            transform.SetPositionAndRotation(room.position, room.rotation);
            currentRoom = room;
            playerStation._EnterStation(room);
        }
    }

    public void OnLocalPlayerAssigned(StationController station)
    {
        stationAssigned = true;
        playerStation = station;
        Initialize();
        Networking.LocalPlayer.Respawn();
        Debug.LogFormat("{0}: OnLocalPlayerAssigned Triggered!");
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer.isMaster)
        {
            GameObject spawnedStation = objectPool.TryToSpawn();
            Networking.SetOwner(player, spawnedStation);
            Networking.SetOwner(player, spawnedStation.GetComponent<StationController>().syncObj.gameObject);
            if (player.isLocal) spawnedStation.GetComponent<StationController>().OnOwnershipTransferred(player);
        }
    }

    public void Update()
    {
        if (playerStation != null && currentRoom != null) Debug.LogFormat("{0}: playerStation: {1} | currentRoom: {2} | stationAssigned: {3}", name, playerStation.name, currentRoom.name, stationAssigned);
    }
}
