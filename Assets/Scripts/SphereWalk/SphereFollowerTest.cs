
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SphereFollowerTest : UdonSharpBehaviour
{
    public Transform staticSphere;
    public Transform movingSphere;
    private VRCPlayerApi localPlayer;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;        
    }

    private void Update()
    {
        Vector3 localPlayerPosition = localPlayer.GetPosition();

        var matrix = movingSphere.worldToLocalMatrix;
        transform.localPosition = matrix.MultiplyPoint(localPlayerPosition);
        transform.localRotation = matrix.rotation * localPlayer.GetRotation();

        //Vector3 pos = ((localPlayerPosition - (movingSphere.position + movingSphere.up * radius)) + (staticSphere.up * radius));

        //var matrix = staticSphere.worldToLocalMatrix;
        //transform.localPosition = matrix.MultiplyPoint(pos);

        //Debug.LogFormat("Player Position: {0}, Up to Player Vector: {1}, Follower Position: {2}", localPlayerPosition, (localPlayerPosition - (movingSphere.position + movingSphere.up * radius)), transform.localPosition);
    }
}
