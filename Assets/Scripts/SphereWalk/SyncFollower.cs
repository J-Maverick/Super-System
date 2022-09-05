
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

// I'm losing my mind...
public class SyncFollower : UdonSharpBehaviour
{
    public StationController station;
    public Transform followerOrigin;

    private void Update()
    {
        if (!station.hasValidPlayer())
        {
            return;
        }
        Vector3 pos = station.syncObj.position;
        Quaternion rot = station.syncObj.rotation;

        var matrix = followerOrigin.localToWorldMatrix;
        Vector3 systemPos = matrix.MultiplyPoint(pos);
        Quaternion systemRot = matrix.rotation * rot;

        transform.SetPositionAndRotation(systemPos, systemRot);
    }
}
