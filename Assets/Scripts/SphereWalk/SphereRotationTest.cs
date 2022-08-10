
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SphereRotationTest : UdonSharpBehaviour
{
    private VRCPlayerApi localPlayer;
    private Vector3 lastPosition;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        lastPosition = localPlayer.GetPosition();
    }

    private void Update()
    {
        Vector3 playerPos = localPlayer.GetPosition();
        Vector3 movement = playerPos - lastPosition;
        float angle = movement.magnitude * (180f / Mathf.PI) / (playerPos - transform.position).magnitude;

        Vector3 rotationAxis = Vector3.Cross(Vector3.up, movement).normalized;
        transform.localRotation = Quaternion.Euler(rotationAxis * -angle) * transform.localRotation;

        lastPosition = playerPos;

        playerPos.y = transform.position.y;
        transform.position = playerPos;
    }
}
