
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[DefaultExecutionOrder(1000000000)]
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class StationController : UdonSharpBehaviour
{
    public VRCStation station;
    public Transform syncObj;
    public FixedTransformSync syncTarget;
    public Transform stationPos;
    public Origin origin = null;
    public Transform currentRoom = null;
    [UdonSynced] public int currentRoomID;

    private VRCPlayerApi _usingPlayer;
    private float nextTime = 0f;
    public float intervalTime = 3f;

    public void _EnterStation(Transform room)
    {
        station.PlayerMobility = VRC.SDKBase.VRCStation.Mobility.Mobile;
        VRCPlayerApi player = Networking.LocalPlayer;
        transform.SetPositionAndRotation(player.GetPosition(), player.GetRotation());
        stationPos.SetPositionAndRotation(player.GetPosition(), player.GetRotation());
        station.UseStation(player);
        _usingPlayer = player;

        currentRoom = room;
        currentRoomID = room.GetComponent<Room>().roomID;

        if (_usingPlayer.isLocal)
        {
            _LogAtInterval(string.Format("{0}: Updating local position", name));
            _UpdateLocal();
            RequestSerialization();
        }

    }

    public void _EnterRoom(Transform room)
    {
        currentRoom = room;
        currentRoomID = room.GetComponent<Room>().roomID;

        RequestSerialization();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (_usingPlayer == null)
        {
            _usingPlayer = Networking.GetOwner(gameObject);
        }
        RequestSerialization();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        _usingPlayer = player;
        if (player.isLocal)
        {
            origin.OnLocalPlayerAssigned(this);
            RequestSerialization();
        }
    }

    public override void OnPreSerialization()
    {
        if (_usingPlayer != null && _usingPlayer.isLocal) currentRoomID = currentRoom.GetComponent<Room>().roomID;
    }

    public override void OnDeserialization()
    {
        if (Utilities.IsValid(origin)) currentRoom = origin.GetRoomByID(currentRoomID);
        if (_usingPlayer == null) _usingPlayer = Networking.GetOwner(gameObject);
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        Debug.LogFormat("{0}: Player {1}[{2}] entered station", name, player.displayName, player.playerId);
        _usingPlayer = player;
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        Debug.LogFormat("{0}: Player {1}[{2}] exited station", name, player.displayName, player.playerId);
        _usingPlayer = null;
        //station.PlayerMobility = VRC.SDKBase.VRCStation.Mobility.Immobilize;
        if (player.isLocal)
        {
            _UpdateLocal();
        }
    }

    public bool hasValidPlayer()
    {
        return Utilities.IsValid(_usingPlayer);
    }

    public void Update()
    {
        if (_usingPlayer == null) _usingPlayer = Networking.GetOwner(gameObject);
        _CheckUsingPlayerIsOwner();
    }

    public override void PostLateUpdate()
    {
        if (gameObject.activeSelf)
        {
            if (origin != null)
            {
                if (currentRoom == null) currentRoom = origin.GetRoomByID(currentRoomID);
            }

            if (!hasValidPlayer())
            {
                _LogAtInterval(string.Format("{0}: No valid player", name));
                _ResetLogInterval();
                return;
            }

            if (_usingPlayer.isLocal)
            {
               _LogAtInterval(string.Format("{0}: Updating local position -- current room: {1} (ID: {2}) -- Owner: {3}[{4}]", name, currentRoom.name, currentRoomID, _usingPlayer.displayName, _usingPlayer.playerId));
               _UpdateLocal();
            }

            else if (currentRoom != null)
            {
               _LogAtInterval(string.Format("{0}: Updating remote position -- current room: {1} (ID: {2}) -- Owner: {3}[{4}]", name, currentRoom.name, currentRoomID, _usingPlayer.displayName, _usingPlayer.playerId));
               _UpdateRemote();
            }
            else _LogAtInterval(string.Format("{0}: Failed to update remote position -- Invalid currentRoom (ID: {1}) -- Owner: {3}[{4}]", name, currentRoom.name, currentRoomID, _usingPlayer.displayName, _usingPlayer.playerId));

            //if (_usingPlayer.GetVelocity().magnitude > 1000.0) _usingPlayer.SetVelocity(Vector3.zero);
        }
        _ResetLogInterval();
    }

    private void _UpdateLocal()
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        Vector3 pos = player.GetPosition();
        Quaternion rot = player.GetRotation();

        stationPos.SetPositionAndRotation(pos, rot);

        var matrix = currentRoom.worldToLocalMatrix;
        Vector3 systemPos = matrix.MultiplyPoint(pos);
        Quaternion systemRot = matrix.rotation * rot;
        syncTarget.SetPositionRotation(systemPos, systemRot);
    }

    private void _UpdateRemote()
    {
        Vector3 pos = syncTarget.position;
        Quaternion rot = syncTarget.rotation;

        var matrix = currentRoom.localToWorldMatrix;

        Vector3 systemPos = matrix.MultiplyPoint(pos);
        Quaternion systemRot = matrix.rotation * rot;

        stationPos.SetPositionAndRotation(systemPos, systemRot);
    }

    private void _LogAtInterval(string message)
    {
        if (Time.realtimeSinceStartup > nextTime)
        {
            Debug.LogFormat("{0}: {1}", name, message);
        }
    }

    private void _ResetLogInterval()
    {
        if (Time.realtimeSinceStartup > nextTime)
        {
            nextTime = Time.realtimeSinceStartup + intervalTime;
        }
    }

    private void _CheckUsingPlayerIsOwner()
    {
        if (Time.realtimeSinceStartup > nextTime && Networking.GetOwner(gameObject) != _usingPlayer)
        {
            Networking.SetOwner(_usingPlayer, gameObject);
            Networking.SetOwner(_usingPlayer, syncObj.gameObject);
            origin.OnLocalPlayerAssigned(this);
        }
        else if (_usingPlayer != null && _usingPlayer.isLocal && origin.playerStation != this) origin.OnLocalPlayerAssigned(this);
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (player.isLocal) RequestSerialization();
    }
}
